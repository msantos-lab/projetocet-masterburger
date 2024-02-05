using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize(Roles = "Entrega,Admin")]
	[Area("Admin")]
	public class AdminEntregaController : Controller {
		private readonly ApplicationDbContext _context;

		public AdminEntregaController(ApplicationDbContext context) {
			_context = context;
		}

		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "EncomendaId") {
			// Filtra as encomendas com status para Entrega
			var encomendas = _context.Encomendas
					.Where(e => e.Status == "Pronto para Entrega" || e.Status == "Em Rota");


			if (!string.IsNullOrWhiteSpace(filter)) {
				encomendas = encomendas.Where(p => p.Status.Contains(filter));
			}

			var model = await PagingList.CreateAsync(encomendas, 10, pageindex, sort, "EncomendaId");
			model.RouteValue = new RouteValueDictionary { { "filter", filter } };
			return View(model);
		}


		[Route("Admin/AdminEntrega/Details")]
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

		[Route("Admin/AdminEntrega/AlterarStatusEncomenda")]
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

				_context.Encomendas.Update(Encomenda);
				await _context.SaveChangesAsync();
			} catch (Exception ex) {
				Console.WriteLine($"Erro ao alterar o status da encomenda: {ex.Message}");
			}

			return RedirectToAction("Details", new { id = Encomenda.EncomendaId });
		}

	}
}
