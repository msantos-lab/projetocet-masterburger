using MasterBurger.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MasterBurger.ViewsModels {
	public class HomeViewModel {
		public IEnumerable<Produto> ProdutosPreferidos { get; set; }
		public string OrderBy { get; set; }
		public List<SelectListItem> OrderByOptions { get; set; }
		public string CategoriaNome { get; set; }
		public int Perfil { get; set; }
    public int UtilizadorId { get; set; }
    public CarrinhoCompra CarrinhoCompra { get; set; }
		public decimal CarrinhoCompraTotal { get; set; }
    public List<Encomenda> Encomendas { get; set; } 
    public Encomenda Encomenda { get; set; }

		public IEnumerable<Banner> Sliders { get; set; }
		public Banner Halfbanner1 { get; set; }
		public Banner Halfbanner2 { get; set; }
		public Banner Banner { get; set; }
	}
}