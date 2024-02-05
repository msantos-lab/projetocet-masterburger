using iTextSharp.text.pdf;
using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Data;

namespace MasterBurger.Controllers {
	public class EncomendaController : Controller {
    private readonly IEncomendaRepository _EncomendaRepository;
    private readonly CarrinhoCompra _carrinhoCompra;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IProdutoRepository _produtoRepository;
		private readonly ICupomRepository _cupomRepository;

		public EncomendaController(IEncomendaRepository EncomendaRepository, CarrinhoCompra carrinhoCompra, UserManager<IdentityUser> userManager, ApplicationDbContext context, IProdutoRepository produtoRepository, ICupomRepository cupomRepository) {
      _EncomendaRepository = EncomendaRepository;
      _carrinhoCompra = carrinhoCompra;
      _userManager = userManager;
      _context = context;
      _produtoRepository = produtoRepository;
			_cupomRepository = cupomRepository;

		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Checkout(string utilizadorId) {
			var user = await _userManager.GetUserAsync(User);
			var itensDoCarrinho = _carrinhoCompra.GetCarrinhoCompraItens();
			decimal carrinhoCompraTotal = Convert.ToDecimal(TempData["CarrinhoCompraTotal"]);

			if (user != null && itensDoCarrinho.Count > 0) {
				var dadosUtilizador = _context.DadosUser
						.Where(dadosUser => dadosUser.UserId == user.Id)
						.SelectMany(dadosUser => _context.DadosUtilizador
								.Where(dadosUtilizador => dadosUtilizador.DadosUtilizadorId == dadosUser.DadosUtilId))
						.ToList();

				var checkoutViewModel = new CheckoutViewModel {
					DadosUtilizador = dadosUtilizador,
					ItensDoCarrinho = itensDoCarrinho,
					CarrinhoCompraTotal = carrinhoCompraTotal,
					CodigoCupom = _carrinhoCompra.CodigoCupomTemporario,
					DescontoPercentual = _carrinhoCompra.DescontoPercentualTemporario 
				};

				return View(checkoutViewModel);
			} else {
				return View("Erro");
			}
		}


		[HttpPost]
		public async Task<IActionResult> Checkout(Encomenda Encomenda) {
			try {
				decimal valorCarrinhoCompraTotal = 0;
				int cupomPercentual = 0;
				string codigoCupom = "";

				if (Request.Cookies.TryGetValue("TotalCompra", out var carrinhoCompraTotal)) {
					valorCarrinhoCompraTotal = decimal.Parse(carrinhoCompraTotal);
				}

				if (Request.Cookies.TryGetValue("DescontoPercentualTemporario", out var descontoPercentualTemporario)) {
					cupomPercentual = int.Parse(descontoPercentualTemporario);
				}

				if (Request.Cookies.TryGetValue("CodigoCupom", out var cCupom)) {
					codigoCupom = cCupom;
				}

				var user = await _userManager.GetUserAsync(User);

				// Atribui informacoes na Encomenda
				string ultimoEncomendaId = _EncomendaRepository.ObterMaiorEncomendaId();
				int novoEncomendaId = Convert.ToInt16(ultimoEncomendaId) + 1;
				string EncomendaIdFormatado = novoEncomendaId.ToString("D3");
				Encomenda.EncomendaId = EncomendaIdFormatado;
				Encomenda.Status = "Encomenda Recebida";
				Encomenda.CodigoCupom = codigoCupom;
				Encomenda.DescontoPercentual = cupomPercentual;

				int totalItensEncomenda = 0;

				// Itens do carrinho de compra
				List<CarrinhoCompraItem> items = _carrinhoCompra.GetCarrinhoCompraItens();
				_carrinhoCompra.CarrinhoCompraItems = items;

				// Verifica se existem itens no carrinho
				if (_carrinhoCompra.CarrinhoCompraItems.Count == 0) {
					ModelState.AddModelError("", "Carrinho vazio.");
				}

				// Calcula o total de itens
				foreach (var item in items) {
					totalItensEncomenda += item.Quantidade;
				}

				Encomenda.UserId = user.Id;
				Encomenda.TotalItensEncomenda = totalItensEncomenda;
				Encomenda.EncomendaTotal = valorCarrinhoCompraTotal;

				if (Encomenda.UserId != null && ModelState.IsValid) {
					_carrinhoCompra.LimparCarrinho();
					_EncomendaRepository.CriarEncomenda(Encomenda);

					// Perfis dos admins
					var admins = await _userManager.GetUsersInRoleAsync("Admin");

					// Envia o email para todos os admins
					foreach (var admin in admins) {
						await EnviarEmailAdmin(Encomenda, admin.Email);
					}

					ViewBag.TotalEncomenda = valorCarrinhoCompraTotal;

					// Apaga Cookies
					Response.Cookies.Delete("DescontoPercentualTemporario");
					Response.Cookies.Delete("CodigoCupom");
					Response.Cookies.Delete("TotalCompra");

					// Envia o email com o PDF
					await EnviarRecibo(Encomenda);

					return View("~/Views/Encomenda/CheckoutCompleto.cshtml", Encomenda);
				}

				return View();
			} catch (Exception ex) {
				return View("Error");
			}
		}



		[HttpPost]
    public async Task<IActionResult> EnviarRecibo(Encomenda Encomenda) {
			// Consulta 1: Informações da encomenda
			var encomenda = await _context.Encomendas
					.Where(p => p.EncomendaId == Encomenda.EncomendaId)
					.FirstOrDefaultAsync();

			// Consulta 2: Dados Utilizador
			var dadosUtilId = await _context.DadosUser
			.Where(d => d.UserId == encomenda.UserId)
			.Select(d => d.DadosUtilId)
			.FirstOrDefaultAsync();

			if (dadosUtilId == "0") {
				return NotFound();
			}

			var dadosUtilizador = await _context.DadosUtilizador
					.Where(d => d.DadosUtilizadorId == dadosUtilId)
					.FirstOrDefaultAsync();

			if (dadosUtilizador == null) {
				return NotFound();
			}


			// Consulta 3:Email
			var user = await _context.Users
					.Where(u => u.Id == encomenda.UserId)
					.FirstOrDefaultAsync();

			if (user == null) {
				return NotFound();
			}


			string templatePath = @"D:\\Program Files\\Microsoft Visual Studio\\repos\\MasterBurger\\wwwroot\\pdfTemplate\\template_masterb.pdf";
      string outputPath = @"D:\\Program Files\\Microsoft Visual Studio\\repos\\MasterBurger\\wwwroot\\pdfs\\dados_encomenda.pdf";

      // Carrega o PDF template
      PdfReader pdfReader = new PdfReader(templatePath);
      using (FileStream fs = new FileStream(outputPath, FileMode.Create)) {

        PdfStamper pdfStamper = new PdfStamper(pdfReader, fs);
        int itemCounter = 1;
        // Substitui um marcador no PDF template com dados dinâmicos
        AcroFields pdfFormFields = pdfStamper.AcroFields;
        pdfFormFields.SetField("EncomendaId", Encomenda.EncomendaId.ToString());
        pdfFormFields.SetField("EncomendaTotal", Encomenda.EncomendaTotal.ToString("C"));

				decimal subtotal = 0;

				//Adiciona Itens da encomenda
				foreach (var item in Encomenda.EncomendaItens) {

					subtotal = item.Produto.Preco * item.Quantidade;

					pdfFormFields.SetField("ProdutoQtd" + itemCounter, item.Quantidade.ToString());
					pdfFormFields.SetField("ProdutoNome" + itemCounter, item.Produto.Nome);
					pdfFormFields.SetField("ProdutoPreco" + itemCounter, item.Produto.Preco.ToString("C"));
					pdfFormFields.SetField("ProdutoSub" + itemCounter, subtotal.ToString("C"));

					itemCounter++;
				}
				pdfStamper.FormFlattening = true; //PDF nao editavel
        pdfStamper.Close();
      }


      byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);

      // Configurar o e-mail
      var message = new MimeMessage();
      message.From.Add(new MailboxAddress("MasterBurger", "monica.santos.24244@formandos.cinel.pt"));
      message.To.Add(new MailboxAddress("", user.Email));
      message.Subject = "Detalhe da Encomenda Nº: " + Encomenda.EncomendaId;

			var builder = new BodyBuilder();
			builder.HtmlBody = $"Prezado(a) {dadosUtilizador.Nome},<br><p>Estamos felizes em confirmar a sua Encomenda Nº {Encomenda.EncomendaId}.</p>";

			// Adiciona Itens da encomenda ao corpo do e-mail
			builder.HtmlBody += "<table width='650px'  style='margin-top: 20px'>";
			builder.HtmlBody += "<thead><tr><th><b>Produto</b></th><th><b>Preço</b></th><th><b>Quantidade</b></th><th><b>Subtotal</b></th></tr></thead><tbody>";

			foreach (var item in encomenda.EncomendaItens) {
				builder.HtmlBody += $"<tr style='border: 1px solid #c1c1c1;'><td style='text-align: center;'>";

				builder.HtmlBody += $"<a href='https://localhost:7178/Produto/Details?ProdutoId={item.Produto.ProdutoId}'><h2 style='font-size: 0.85rem; color: #FC6727; text-decoration: none; text-transform: uppercase;'>{item.Produto.Nome}</h2></a></td>";

				builder.HtmlBody += $"<td style='text-align: center;'>{item.Produto.Preco.ToString("c")}</td><td style='text-align: center;'>{item.Quantidade}</td><td style='text-align: center;'>{(item.Quantidade * item.Produto.Preco).ToString("c")}</td></tr>";
			}

			builder.HtmlBody += "</tbody></table>";

			// Adiciona o subtotal ao corpo do e-mail
			builder.HtmlBody += $"<table><tfoot><tr><td><b>TOTAL:</b> </td><td><b>{encomenda.EncomendaTotal.ToString("c")}</b></td></tr></tfoot></table><br>";
			builder.HtmlBody += "<p>Anexamos o detalhe da mesma em formato PDF para sua referência.</p><p>Agradecemos pela sua preferência e estamos à disposição para qualquer assistência adicional.</p><br><br>Atenciosamente,<br>MasterBurger";

      var pdfAttachment = new MimePart("application", "pdf") {
        Content = new MimeContent(new MemoryStream(fileBytes), ContentEncoding.Default),
        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
        ContentTransferEncoding = ContentEncoding.Base64,
        FileName = "dados_encomenda.pdf"
			};

      builder.Attachments.Add(pdfAttachment);
      message.Body = builder.ToMessageBody();

      try {
        using (var client = new MailKit.Net.Smtp.SmtpClient()) {
          client.Connect("smtp.office365.com", 587, false);
          client.Authenticate("monica.santos.24244@formandos.cinel.pt", "301013Fm");
          client.Send(message);
          client.Disconnect(true);

          Console.WriteLine("E-mail enviado com sucesso!");
        }
      } catch (Exception ex) {
        Console.WriteLine("Ocorreu um erro ao enviar o e-mail: " + ex.Message);
      }
      System.IO.File.Delete(outputPath);
      return View("~/Views/Encomenda/CheckoutCompleto.cshtml", Encomenda);
    }


		private async Task EnviarEmailAdmin(Encomenda encomenda, string adminEmail) {
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("MasterBurger", "monica.santos.24244@formandos.cinel.pt"));
			message.To.Add(new MailboxAddress("", adminEmail));
			message.Subject = "Nova Encomenda Recebida - Nº: " + encomenda.EncomendaId;

			var builder = new BodyBuilder();
			builder.HtmlBody = $"Olá,<br><br>Foi recebida uma nova Encomenda Nº {encomenda.EncomendaId}.<br><br><br>";


			builder.HtmlBody += "<table width='650px' style='margin-top: 20px'>";
			builder.HtmlBody += "<thead><tr><th><b>Produto</b></th><th><b>Preço</b></th><th><b>Quantidade</b></th><th><b>Subtotal</b></th></tr></thead><tbody>";

			foreach (var item in encomenda.EncomendaItens) {
				builder.HtmlBody += $"<tr style='border: 1px solid #c1c1c1'><td style='text-align: center;'>";				

				builder.HtmlBody += $"<a href='https://localhost:7178/Produto/Details?ProdutoId={item.Produto.ProdutoId}'><h2 style='font-size: 0.85rem; color: #FC6727; text-decoration: none; text-transform: uppercase;'>{item.Produto.Nome}</h2></a></td>";

				builder.HtmlBody += $"<td style='text-align: center;'>{item.Produto.Preco.ToString("c")}</td><td style='text-align: center;'>{item.Quantidade}</td><td style='text-align: center;'>{(item.Quantidade * item.Produto.Preco).ToString("c")}</td></tr>";
			}

			builder.HtmlBody += "</tbody></table>";

			// Adiciona o subtotal ao corpo do e-mail
			builder.HtmlBody += $"<table><tfoot><tr><td><b>TOTAL:</b> </td><td><b>{encomenda.EncomendaTotal.ToString("c")}</b></td></tr></tfoot></table>";

			// Adiciona o restante do corpo do e-mail
			builder.HtmlBody += "<br><p>Confira os detalhes no <a href='https://localhost:7178/Identity/Account/LoginAdmin' target='_blank'>Dashboard da MasterBurger</a>.</p><br>";

			message.Body = builder.ToMessageBody();

			try {
				using (var client = new MailKit.Net.Smtp.SmtpClient()) {
					client.Connect("smtp.office365.com", 587, false);
					client.Authenticate("monica.santos.24244@formandos.cinel.pt", "301013Fm");
					client.Send(message);
					client.Disconnect(true);

					Console.WriteLine("E-mail enviado com sucesso!");
				}
			} catch (Exception ex) {
				// Captura qualquer exceção que possa ocorrer durante o envio
				Console.WriteLine("Ocorreu um erro ao enviar o e-mail: " + ex.Message);
			}
		}
	}
}

