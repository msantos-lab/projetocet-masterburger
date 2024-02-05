using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize]
	[Area("Admin")]
	public class AdminEncomendasController : Controller {
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public AdminEncomendasController(ApplicationDbContext context, UserManager<IdentityUser> userManager) {
			_context = context;
			_userManager = userManager;
		}


		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "EncomendaId") {
			var resultado = _context.Encomendas.AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter)) {
				resultado = resultado.Where(p => p.Status.Contains(filter));
			}


			var model = await PagingList.CreateAsync(resultado, 10, pageindex, sort, "EncomendaId");
			model.RouteValue = new RouteValueDictionary { { "filter", filter } };
			return View(model);
		}


		[Route("Admin/AdminEncomendas/Details")]
		public async Task<IActionResult> Details(string id) {
			if (id == null) {
				return NotFound();
			}

			var userId = await _context.Encomendas
					.Where(p => p.EncomendaId == id)
					.Select(p => p.UserId)
					.FirstOrDefaultAsync();

			if (userId == null) {
				return NotFound();
			}

			var EncomendaDetalhesViewModel = new AdminEcomendaDetalhesViewModel();

			// Consulta 1: Informações da encomenda
			var Encomenda = await _context.Encomendas
				.Where(p => p.EncomendaId == id)
				.FirstOrDefaultAsync();

			if (Encomenda == null) {
				return NotFound();
			}
			EncomendaDetalhesViewModel.EncomendaId = Encomenda.EncomendaId;
			EncomendaDetalhesViewModel.EncomendaTotal = Encomenda.EncomendaTotal;
			EncomendaDetalhesViewModel.TotalItensEncomenda = Encomenda.TotalItensEncomenda;
			EncomendaDetalhesViewModel.EncomendaRealizada = Encomenda.EncomendaRealizada;
			EncomendaDetalhesViewModel.EncomendaConcluida = Encomenda.EncomendaConcluida;
			EncomendaDetalhesViewModel.Status = Encomenda.Status;
			EncomendaDetalhesViewModel.CodigoCupom = Encomenda.CodigoCupom;
			EncomendaDetalhesViewModel.DescontoPercentual = Encomenda.DescontoPercentual;

			// Consulta 2: Dados Utilizador
			var dadosUtilId = await _context.DadosUser
			.Where(d => d.UserId == Encomenda.UserId)
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
			EncomendaDetalhesViewModel.Nome = dadosUtilizador.Nome;
			EncomendaDetalhesViewModel.Apelido = dadosUtilizador.Apelido;
			EncomendaDetalhesViewModel.NIF = dadosUtilizador.NIF;
			EncomendaDetalhesViewModel.Morada = dadosUtilizador.Morada;
			EncomendaDetalhesViewModel.Localidade = dadosUtilizador.Localidade;
			EncomendaDetalhesViewModel.CodigoPostal = dadosUtilizador.CodigoPostal;
			EncomendaDetalhesViewModel.Telemovel = dadosUtilizador.Telemovel;


			// Consulta 3:Email
			var user = await _context.Users
					.Where(u => u.Id == Encomenda.UserId)
					.FirstOrDefaultAsync();

			if (user == null) {
				return NotFound();
			}
			EncomendaDetalhesViewModel.Email = user.Email;


			// Consulta 4: Produtos
			var produtos = await ObterProdutosDoEncomenda(id);

			if (produtos == null) {
				return NotFound();
			}
			EncomendaDetalhesViewModel.Produtos = produtos;


			if (EncomendaDetalhesViewModel == null) {
				return NotFound();
			}

			return View(EncomendaDetalhesViewModel);
		}

		private async Task<List<AdminProdutosEncomendaViewModel>> ObterProdutosDoEncomenda(string EncomendaId) {
			var produtos = await _context.EncomendaDetalhes
					.Where(pd => pd.EncomendaId == EncomendaId)
					.Select(pd => new AdminProdutosEncomendaViewModel {
						ProdutoId = pd.ProdutoId,
						Nome = pd.Produto.Nome,
						Preco = pd.Preco,
						Quantidade = pd.Quantidade
					})
					.ToListAsync();

			return produtos;
		}

		[Route("Admin/AdminEncomendas/AlterarStatusEncomenda")]
		public async Task<IActionResult> AlterarStatusEncomenda(string id, string status) {
			if (id == null) {
				return NotFound();
			}

			var Encomenda = await _context.Encomendas.FindAsync(id);

			if (Encomenda == null) {
				return NotFound();
			}

			try {
				Encomenda.Status = status;

				if (status == "Entregue") {
					Encomenda.EncomendaConcluida = DateTime.Now;
				}

				if (status == "Em Rota") {
					await NotificarClienteEmRota(id);
				}

				_context.Encomendas.Update(Encomenda);
				await _context.SaveChangesAsync();
			} catch (Exception ex) {
				Console.WriteLine($"Erro ao alterar o status da encomenda: {ex.Message}");
			}

			return RedirectToAction("Details", new { id = Encomenda.EncomendaId });
		}



		[HttpGet]
		public async Task<IActionResult> NotificarClienteEmRota(string id) {
			// Consulta 1: Informações da encomenda
			var encomenda = await _context.Encomendas
				.Where(p => p.EncomendaId == id)
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

			// Perfis dos admins
			var admins = await _userManager.GetUsersInRoleAsync("Admin");

			// Configurar o e-mail
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("MasterBurger", "monica.santos.24244@formandos.cinel.pt"));
			message.To.Add(new MailboxAddress("", user.Email));
			message.Subject = "A sua encomenda saiu para ser entregue";

			// Envia o email para todos os admins
			foreach (var admin in admins) {
				message.Bcc.Add(new MailboxAddress("", admin.Email));
			}

			var builder = new BodyBuilder();

			builder.HtmlBody = $"Prezado(a) {dadosUtilizador.Nome},<br><br><p>Gostaríamos de informar que a sua <b>Encomenda Nº {encomenda.EncomendaId}</b> está agora <span style='color: #FC6727;'>Em Rota</span>! </p><p>Nossos dedicados entregadores estão a caminho para garantir que suas delícias cheguem até você em breve</p><p>Agradecemos pela preferência e esperamos que desfrute da sua experiência gastronômica.</p><br><br>Atenciosamente,<br>MasterBurger";

			message.Body = builder.ToMessageBody();

			try {
				using (var client = new MailKit.Net.Smtp.SmtpClient()) {
					client.Connect("smtp.office365.com", 587, false);
					client.Authenticate("monica.santos.24244@formandos.cinel.pt", "301013Fm");
					client.Send(message);
					client.Disconnect(true);

					Console.WriteLine("E-mail enviado com sucesso!");
					TempData["Mensagem"] = "E-mail enviado com sucesso!";
				}
			} catch (Exception ex) {
				Console.WriteLine("Ocorreu um erro ao enviar o e-mail: " + ex.Message);
				TempData["Mensagem"] = "Erro ao enviar o e-mail.";
			}


			return RedirectToAction(nameof(Details), new { id });
		}

	}
}
