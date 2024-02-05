using MasterBurger.Models;

namespace MasterBurger.ViewsModels {
	public class EncomendaProdutosViewModel {
		public Encomenda Encomenda { get; set; }
		public IEnumerable<EncomendaDetalhe> EncomendaDetalhes { get; set; }
		public List<CarrinhoCompraItem> CarrinhoCompraItem { get; set; }
		public string Nome { get; set; }
	}
}
