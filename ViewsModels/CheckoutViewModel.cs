using MasterBurger.Models;

namespace MasterBurger.ViewsModels {
	public class CheckoutViewModel {
		public List<DadosUtilizador> DadosUtilizador { get; set; }
		public List<CarrinhoCompraItem> ItensDoCarrinho { get; set; }
		public decimal CarrinhoCompraTotal { get; set; }

		public string CodigoCupom { get; set; }
		public decimal DescontoPercentual { get; set; }

	}

}
