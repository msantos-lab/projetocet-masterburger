using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize(Roles = "Admin")]
	[Area("Admin")]
	[Route("admin/dashboard")]
	public class DashboardController : Controller {

		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly IRelatoriosRepository _relatoriosRepository;

		public DashboardController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ApplicationDbContext context, IRelatoriosRepository relatoriosRepository) {
			_signInManager = signInManager;
			_userManager = userManager;
			_relatoriosRepository = relatoriosRepository;
			_context = context;
		}

		[HttpGet("index")]
		public async Task<IActionResult> Index(int pageindex = 1, string sort = "EncomendaId") {
			var resultado = _context.Encomendas
		.Where(encomenda => encomenda.Status == "Encomenda Recebida")
		.AsQueryable();
			var model = await PagingList.CreateAsync(resultado, 5, pageindex, sort, "EncomendaId");

			//Gráficos
			DateTime primeiroDiaDoAno = new DateTime(DateTime.Now.Year, 1, 1);
			DateTime ultimoDiaDoAno = new DateTime(DateTime.Now.Year, 12, 31);

			var vendasTotais = _relatoriosRepository.ObterVendasTotais(primeiroDiaDoAno, ultimoDiaDoAno);
			var vendasPorCategoria = _relatoriosRepository.ObterVendasPorCategoria(DateTime.Now.AddDays(-30), DateTime.Now);
			var produtosMaisVendidos = _relatoriosRepository.ObterProdutosMaisVendidos(DateTime.Now.AddDays(-30), DateTime.Now);

			ViewData["VendasTotais"] = vendasTotais;
			ViewData["VendasPorCategoria"] = vendasPorCategoria;
			ViewData["ProdutosMaisVendidos"] = produtosMaisVendidos;

			return View(model);
		}


		[Route("login")]
		public async Task<IActionResult> LoginAdmin(LoginInputModel model) {
			if (ModelState.IsValid) {
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded) {
					return RedirectToAction("Index", "Dashboard");
				}
			}
			return View(model);
		}

	}
}
