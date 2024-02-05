using MasterBurger.Models;

namespace MasterBurger.ViewsModels {
	public class CarrinhoCompraViewModel {
		public CarrinhoCompra CarrinhoCompra { get; set; }
		public decimal CarrinhoCompraTotal { get; set; }

		public string CodigoCupom { get; set; }
		public decimal DescontoPercentual { get; set; }

	}

}
