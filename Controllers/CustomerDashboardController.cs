using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MasterBurger.Controllers {
	[Authorize]
	public class CustomerDashboardController : Controller {
		private readonly UserManager<IdentityUser> _userManager;

		public CustomerDashboardController(UserManager<IdentityUser> userManager) {
			_userManager = userManager;
		}

		public async Task<IActionResult> Index() {
			//utilizador atual
			var user = await _userManager.GetUserAsync(User);

			var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

			// Se o e-mail não estiver confirmado, exibi noificacao
			if (!isEmailConfirmed) {
				ViewData["EmailNaoConfirmado"] = true;
			}

			return View();
		}
	}

}
