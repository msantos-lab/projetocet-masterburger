using MasterBurger.Models;

namespace MasterBurger.ViewsModels {
	public class BannerPrincipalViewModel {
		public IEnumerable<Banner> Banners { get; set; }
		public IEnumerable<Produto> Produtos { get; set; }
	}
}
