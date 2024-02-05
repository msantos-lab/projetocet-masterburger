using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Mvc;


namespace MasterBurger.Controllers {
	public class CarrinhoCompraController : Controller {
		private readonly IProdutoRepository _produtoRepository;
		private readonly CarrinhoCompra _carrinhoCompra;
		private readonly ApplicationDbContext _context;

		public CarrinhoCompraController(IProdutoRepository produtoRepository, CarrinhoCompra carrinhoCompra, ApplicationDbContext context) {
			_produtoRepository = produtoRepository;
			_carrinhoCompra = carrinhoCompra;
			_context = context;
		}

		[HttpPost]
		public IActionResult AplicarCupom(CarrinhoCompraViewModel model) {

			var prazoCookies = new CookieOptions {
				Expires = DateTime.UtcNow.AddMinutes(30) // Os dados do cupom expiram em 30 minutos
			};

			if (!string.IsNullOrEmpty(model.CodigoCupom)) {
				// Valida e aplica o cupom
				_carrinhoCompra.AplicarDesconto(model.CodigoCupom);

				// Armazena temporariamente o desconto percentual em um cookie
				Response.Cookies.Append("DescontoPercentualTemporario", _carrinhoCompra.DescontoPercentualTemporario.ToString(), prazoCookies);
				Response.Cookies.Append("CodigoCupom", model.CodigoCupom, prazoCookies);
			}

			if (VerificarCupom(model.CodigoCupom)) {
				TempData["MensagemVoucher"] = "Voucher inserido com sucesso.";

			} else {
				TempData["MensagemVoucher"] = "Voucher inválido ou expirado.";

			}

			return RedirectToAction("Index");
		}

		public IActionResult Index() {
			var itens = _carrinhoCompra.GetCarrinhoCompraItens();
			_carrinhoCompra.CarrinhoCompraItems = itens;
			decimal carrinhoCT = 0;

			var prazoCookies = new CookieOptions {
				Expires = DateTime.UtcNow.AddMinutes(30) //Os dados do cupom expiram em 30 minutos
			};

			// Compra com desconto
			if (Request.Cookies.TryGetValue("DescontoPercentualTemporario", out string descontoPercentualTemporario)) {
				if (decimal.TryParse(descontoPercentualTemporario, out decimal descontoPercentual)) {

					decimal carrinhoTotal = _carrinhoCompra.GetCarrinhoCompraTotal();

					var carrinhoCompraVM = new CarrinhoCompraViewModel {
						CarrinhoCompra = _carrinhoCompra,
						CarrinhoCompraTotal = carrinhoTotal * (1 - (descontoPercentual / 100))
					};
					carrinhoCT = carrinhoTotal * (1 - (descontoPercentual / 100));
					TempData["CarrinhoCompraTotal"] = carrinhoCT.ToString();
					Response.Cookies.Append("TotalCompra", carrinhoCT.ToString(), prazoCookies);

					if (itens.Count == 0) {
						return View("CarrinhoVazio");
					}

					return View(carrinhoCompraVM);
				}
			}

			// Compra sem desconto
			var carrinhoCompraVMWithoutDiscount = new CarrinhoCompraViewModel {
				CarrinhoCompra = _carrinhoCompra,
				CarrinhoCompraTotal = _carrinhoCompra.GetCarrinhoCompraTotal()
			};
			carrinhoCT = _carrinhoCompra.GetCarrinhoCompraTotal();
			TempData["CarrinhoCompraTotal"] = carrinhoCT.ToString();
			Response.Cookies.Append("TotalCompra", carrinhoCT.ToString(), prazoCookies);

			if (itens.Count == 0) {
				return View("CarrinhoVazio");
			}


			if (TempData.ContainsKey("MensagemVoucher")) {
				ViewData["MensagemVoucher"] = TempData["MensagemVoucher"].ToString();
			}

			return View(carrinhoCompraVMWithoutDiscount);
		}


		public bool VerificarCupom(string codigoCupom) {
			var cupom = _context.Cupons.SingleOrDefault(c => c.Codigo == codigoCupom);

			if (cupom != null && cupom.Status && cupom.DataInicio <= DateTime.Now && cupom.DataFim >= DateTime.Now) {
				return true; // válido
			}

			// Verifica se o cupom expirou
			if (cupom.DataFim < DateTime.Now) {
				// Se expirou, atualiza o status para false
				cupom.Status = false;
				_context.SaveChanges();
			}

			Response.Cookies.Delete("DescontoPercentualTemporario");
			Response.Cookies.Delete("CodigoCupom");
			Response.Cookies.Delete("TotalCompra");
			return false; // inválido
		}


		public IActionResult RemoverCupom() {
			Response.Cookies.Delete("DescontoPercentualTemporario");
			Response.Cookies.Delete("CodigoCupom");
			Response.Cookies.Delete("TotalCompra");

			return RedirectToAction("Index");
		}


		//[HttpGet]
		public IActionResult DetalheAdicionarItemNoCarrinhoCompra(int produtoId, int quantidade) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null && quantidade > 0) {
				var user = this.User;
				_carrinhoCompra.DetalheAdicionarAoCarrinho(produtoSelecionado, user, quantidade);
			}

			return RedirectToAction("Index");
		}


		//[HttpGet]
		public IActionResult AdicionarItemNoCarrinhoCompra(int produtoId) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null) {
				var user = this.User;
				_carrinhoCompra.AdicionarAoCarrinho(produtoSelecionado, user);
			}
			return RedirectToAction("Index");
		}


		public IActionResult RemoverItemDoCarrinhoCompra(int produtoId) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null) {
				_carrinhoCompra.RemoverDoCarrinho(produtoSelecionado);

			}
			return RedirectToAction("Index");
		}

		public IActionResult ExcluirItemDoCarrinhoCompra(int produtoId) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null) {
				_carrinhoCompra.ExcluirDoCarrinho(produtoSelecionado);

				Response.Cookies.Delete("DescontoPercentualTemporario");
				Response.Cookies.Delete("CodigoCupom");
				Response.Cookies.Delete("TotalCompra");
			}
			return RedirectToAction("Index");
		}

		public IActionResult AumentarItemDoCarrinhoCompra(int produtoId, string quantidadeDesejada) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null) {
				// Verifica se aumento da quantidade ultrapassa o stock disponível no carrinho
				if (_carrinhoCompra.VerificarStockDisponivel(produtoSelecionado, Convert.ToInt32(quantidadeDesejada))) {
					_carrinhoCompra.AumentarQuantidade(produtoSelecionado, Convert.ToInt32(quantidadeDesejada));
				} else {
					TempData["ErrorMessage"] = $"Desculpe, mas não é possível aumentar a quantidade do {produtoSelecionado.Nome}.";
				}
			}

			return RedirectToAction("Index");
		}


		public IActionResult DiminuirItemDoCarrinhoCompra(int produtoId) {
			var produtoSelecionado = _produtoRepository.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produtoSelecionado != null) {
				_carrinhoCompra.DiminuirQuantidade(produtoSelecionado);
			}
			return RedirectToAction("Index");
		}

		public IActionResult CarrinhoVazio() {
			return View();
		}


		public bool VerificarStockDisponivel(Produto produto, int quantidadeDesejada) {
			var itemNoCarrinho = _carrinhoCompra.CarrinhoCompraItems
					.FirstOrDefault(item => item.Produto.ProdutoId == produto.ProdutoId);

			if (itemNoCarrinho != null) {
				// Verifica se a soma da quantidade desejada com a quantidade no carrinho ultrapassa o disponível
				return (itemNoCarrinho.Quantidade + quantidadeDesejada) <= produto.Quantidade;
			}

			// Se o item não estiver no carrinho, verifica se a quantidade desejada é menor ou igual ao total
			return quantidadeDesejada <= produto.Quantidade;
		}

	}
}
