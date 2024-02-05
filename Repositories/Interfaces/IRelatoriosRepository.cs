using MasterBurger.Areas.Admin.Views.ViewsModels;

namespace MasterBurger.Repositories.Interfaces {
	public interface IRelatoriosRepository {
		List<VendasTotaisViewModel> ObterVendasTotais(DateTime dataInicio, DateTime dataFim);
		List<VendasPorCategoriaViewModel> ObterVendasPorCategoria(DateTime dataInicio, DateTime dataFim);
		List<ProdutosMaisVendidosViewModel> ObterProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim);

		List<object> RelatorioVendasTotais(DateTime dataInicio, DateTime dataFim);
		List<object> RelatorioVendasPorCategoria(DateTime dataInicio, DateTime dataFim);
		List<object> RelatorioProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim);
	}
}
