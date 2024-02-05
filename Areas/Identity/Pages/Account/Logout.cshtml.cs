// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MasterBurger.Areas.Identity.Pages.Account {
	public class LogoutModel : PageModel {
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly ILogger<LogoutModel> _logger;
		private readonly UserManager<IdentityUser> _userManager;

		public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, UserManager<IdentityUser> userManager) {
			_signInManager = signInManager;
			_logger = logger;
			_userManager = userManager;
		}

		public async Task<IActionResult> OnPost(string returnUrl = null) {
			await _signInManager.SignOutAsync();
			_logger.LogInformation("User logged out.");

			var user = await _userManager.GetUserAsync(User);

			if (user != null) {

				var roles = await _userManager.GetRolesAsync(user);

				// Verifica se o utilizador tem o papel Admin, Cozinha ou Entrega
				if (roles.Contains("Admin") || roles.Contains("Cozinha") || roles.Contains("Entrega")) {

					return Redirect("/Identity/Account/LoginAdmin");
				}
			}

			// Redirecionar para a URL da página principal do site se nenhum papel correspondente for encontrado
			return LocalRedirect("/");
		}

	}
}
