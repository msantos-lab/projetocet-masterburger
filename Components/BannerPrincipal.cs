using MasterBurger.Data;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Mvc;

namespace MasterBurger.Components {
	public class BannerPrincipal : ViewComponent {

		private readonly IBannerRepository _bannerRepository;
		private readonly ApplicationDbContext _context;

		public BannerPrincipal(IBannerRepository bannerRepository, ApplicationDbContext context) {
			_bannerRepository = bannerRepository;
			_context = context;
		}

		public IViewComponentResult Invoke() {
			var banners = _bannerRepository.Banners.ToList();
			var produtos = _context.Produtos.ToList();

			var viewModel = new BannerPrincipalViewModel {
				Banners = banners,
				Produtos = produtos
			};

			return View(viewModel);
		}

	}
}
