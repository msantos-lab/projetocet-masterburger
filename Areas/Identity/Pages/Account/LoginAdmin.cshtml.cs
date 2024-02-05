// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Plugins;

namespace MasterBurger.Areas.Identity.Pages.Account {
	public class LoginAdminModel : PageModel {
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager; 

		private readonly ILogger<LoginModel> _logger;

		public LoginAdminModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager) {
			_signInManager = signInManager;
			_userManager = userManager;
			_logger = logger;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<AuthenticationScheme> ExternalLogins { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ReturnUrl { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string ErrorMessage { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel {
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[Display(Name = "Palavra-passe")]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Display(Name = "Lembrar-me?")]
			public bool RememberMe { get; set; }
		}

    public async Task OnGetAsync() {
		
		}

		public async Task<IActionResult> OnPostAsync() {
			if (ModelState.IsValid) {
				var user = await _userManager.FindByNameAsync(Input.Email);
				if (user != null) {
					var roles = await _userManager.GetRolesAsync(user);

					if (roles.Contains("Cliente")) {
						// Bloqueia o login para clientes 
						ViewData["ErrorMessage"] = "Login não permitido para esta função. Inicie a sessão <a href='/Identity/Account/Login'>aqui</a>";

						return Page();
					}

					var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

					if (result.Succeeded) {
						if (roles.Contains("Admin")) {
							_logger.LogInformation("Admin user logged in.");
							string redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
							return LocalRedirect(redirectUrl);
						} else if (roles.Contains("Cozinha")) {
							_logger.LogInformation("Cozinha user logged in.");
							string redirectUrl = Url.Action("Index", "AdminCozinha", new { area = "Admin" });
							return LocalRedirect(redirectUrl);
						} else if (roles.Contains("Entrega")) {
							_logger.LogInformation("Cozinha user logged in.");
							string redirectUrl = Url.Action("Index", "AdminEntrega", new { area = "Admin" });
							return LocalRedirect(redirectUrl);
						}

					} else if (result.IsLockedOut) {
						_logger.LogWarning("User account locked out.");
						return RedirectToPage("./Lockout");
					} else {
						ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
						return Page();
					}
				} else {
					// Utilizador não encontrado
					ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
					return Page();
				}
			}

			// Se chegamos até aqui, algo falhou; redisplay o formulário de login.
			return Page();
		}

	}
}
