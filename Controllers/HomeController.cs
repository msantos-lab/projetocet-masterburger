using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MasterBurger.Controllers {
	public class HomeController : Controller {

		private readonly ILogger<HomeController> _logger;
		private readonly IProdutoRepository _produtoRepository;
		private readonly IBannerRepository _bannerRepository;

		private readonly UserManager<IdentityUser> _userManager;

		public ILogger<HomeController> Logger { get; }
		private readonly ApplicationDbContext _context;

		public HomeController(ILogger<HomeController> logger, IProdutoRepository produtoRepository, IBannerRepository bannerRepository, ApplicationDbContext context, UserManager<IdentityUser> userManager) {
			_logger = logger;
			_produtoRepository = produtoRepository;
			_bannerRepository = bannerRepository;
			_context = context;
			_userManager = userManager;
		}

		public IActionResult Index(string orderby) {
			var homeViewModel = new HomeViewModel();

			homeViewModel.ProdutosPreferidos = Recentes();

			if (orderby == "ordenaraz") {
				homeViewModel.ProdutosPreferidos = OrdenarAZ();
			} else if (orderby == "ordenarza") {
				homeViewModel.ProdutosPreferidos = OrdenarZA();
			} else if (orderby == "preco-asc") {
				homeViewModel.ProdutosPreferidos = PrecoAsc();
			} else if (orderby == "preco-desc") {
				homeViewModel.ProdutosPreferidos = PrecoDesc();
			}

			foreach (var produto in homeViewModel.ProdutosPreferidos) {
				produto.CategoriaNome = ObterNomeCategoria(produto.CategoriaId);
			}

			homeViewModel.OrderByOptions = GetOrderByOptions();
			homeViewModel.OrderBy = orderby;

			homeViewModel.Sliders = _bannerRepository.ObterBannersPorPosicao("slider");
			homeViewModel.Halfbanner1 = _bannerRepository.ObterBannersPorPosicao("halfbanner1").FirstOrDefault();
			homeViewModel.Halfbanner2 = _bannerRepository.ObterBannersPorPosicao("halfbanner2").FirstOrDefault();
			homeViewModel.Banner = _bannerRepository.ObterBannersPorPosicao("banner").FirstOrDefault();

			return View(homeViewModel);
		}


		private List<Produto> Recentes() {
			List<Produto> produtosPreferidos = _context.Produtos.Include(p => p.Reviews)
							.Where(p => p.IsDestaque)
							.OrderBy(p => p.ProdutoId)
							.ToList();

			return produtosPreferidos;
		}

		private List<Produto> OrdenarAZ() {
			List<Produto> produtosPreferidos = _context.Produtos.Include(p => p.Reviews)
					.Where(p => p.IsDestaque)
					.OrderBy(p => p.Nome)
					.ToList();

			return produtosPreferidos;
		}

		private List<Produto> OrdenarZA() {
			List<Produto> produtosPreferidos = _context.Produtos.Include(p => p.Reviews)
					.Where(p => p.IsDestaque)
					.OrderByDescending(p => p.Nome)
					.ToList();

			return produtosPreferidos;
		}

		private List<Produto> PrecoDesc() {
			List<Produto> produtosPreferidos = _context.Produtos.Include(p => p.Reviews)
					.Where(p => p.IsDestaque)
					.OrderByDescending(p => p.Preco)
					.ToList();

			return produtosPreferidos;
		}

		private List<Produto> PrecoAsc() {
			List<Produto> produtosPreferidos = _context.Produtos.Include(p => p.Reviews)
					.Where(p => p.IsDestaque)
					.OrderBy(p => p.Preco)
					.ToList();

			return produtosPreferidos;
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}


		private List<SelectListItem> GetOrderByOptions() {
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "Ordenar por", Value = "menu_order" },
				new SelectListItem { Text = "--", Value = "recentes" },
				new SelectListItem { Text = "A-Z", Value = "ordenaraz" },
				new SelectListItem { Text = "Z-A", Value = "ordenarza" },
				new SelectListItem { Text = "Preço (mais baixo)", Value = "preco-desc" },
				new SelectListItem { Text = "Preço (mais alto)", Value = "preco-asc" }
		};
		}

		public IActionResult Privacidade() {
			return View();
		}

		public IActionResult CondicoesAvaliacoes() {
			return View();
		}

		public IActionResult Sobre() {
			return View();
		}


		private string ObterNomeCategoria(int categoriaId) {
			var categoria = _context.Categorias
					.Where(c => c.CategoriaId == categoriaId)
					.Select(c => c.CategoriaNome)
					.FirstOrDefault();

			return categoria ?? "Categoria não encontrada";
		}

	}
}