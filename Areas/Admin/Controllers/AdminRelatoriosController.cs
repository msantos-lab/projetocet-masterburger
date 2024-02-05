using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasterBurger.Areas.Admin.Controllers {

	[Authorize]
	[Area("Admin")]
	[Route("Admin/Relatorios")]
	public class AdminRelatoriosController : Controller {

		private readonly ApplicationDbContext _context;
		private readonly IRelatoriosRepository _relatoriosRepository;

		public AdminRelatoriosController(ApplicationDbContext context, IRelatoriosRepository relatoriosRepository) {
			_relatoriosRepository = relatoriosRepository;
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Index(DateTime? inicioData, DateTime? fimData, string relatorioSelecionado) {
			var viewModel = new RelatoriosViewModel {
				DataInicio = inicioData ?? DateTime.Today.AddDays(-7),
				DataFim = fimData ?? DateTime.Today,
				RelatorioSelecionado = relatorioSelecionado
			};

			switch (relatorioSelecionado) {
				case "Vendas Totais":
					viewModel.DadosRelatorio = _relatoriosRepository.RelatorioVendasTotais(viewModel.DataInicio, viewModel.DataFim);
					break;
				case "Vendas Por Categoria":
					viewModel.DadosRelatorio = _relatoriosRepository.RelatorioVendasPorCategoria(viewModel.DataInicio, viewModel.DataFim);
					break;
				case "Produtos Mais Vendidos":
					viewModel.DadosRelatorio = _relatoriosRepository.RelatorioProdutosMaisVendidos(viewModel.DataInicio, viewModel.DataFim);
					break;
				default:
					break;
			}

			return View(viewModel);
		}
	}
}