// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MasterBurger.Areas.Identity.Pages.Account {
	public class RegisterModel : PageModel {
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager; 

		private readonly IUserStore<IdentityUser> _userStore;
		private readonly IUserEmailStore<IdentityUser> _emailStore;
		private readonly ILogger<RegisterModel> _logger;
		private readonly IEmailSender _emailSender;

		public RegisterModel(
				UserManager<IdentityUser> userManager,
				IUserStore<IdentityUser> userStore,
				SignInManager<IdentityUser> signInManager,
				RoleManager<IdentityRole> roleManager,
				ILogger<RegisterModel> logger,
				IEmailSender emailSender) {
			_userManager = userManager;
			_userStore = userStore;
			_emailStore = GetEmailStore();
			_signInManager = signInManager;
			_roleManager = roleManager;
			_logger = logger;
			_emailSender = emailSender;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }
		public SelectList RoleOptions { get; set; } 

		//[BindProperty]
		//public string SelectedRole { get; set; } 


		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ReturnUrl { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
			[EmailAddress(ErrorMessage = "O campo precisa ser um endereço de email válido.")]
			[Display(Name = "Email")]
			public string Email { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
			[DataType(DataType.Password)]
			[Display(Name = "Palavra-passe")]
			public string Password { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[DataType(DataType.Password)]
			[Display(Name = "Confirme a Palavra-passe")]
			[Compare("Password", ErrorMessage = "A palavra-passe e a palavra-passe de confirmação não correspondem.")]
			public string ConfirmPassword { get; set; }
		}


		public async Task OnGetAsync(string returnUrl = null) {
			ReturnUrl = returnUrl;
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			RoleOptions = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
		}

		public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
			returnUrl ??= Url.Content("~/");
			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			if (ModelState.IsValid) {
				var user = CreateUser();

				await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
				await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded) {
					_logger.LogInformation("O utilizador criou uma nova conta com uma palavra-passe.");

					var userId = await _userManager.GetUserIdAsync(user);
					await _userManager.AddToRoleAsync(user, "Cliente");


					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
					var callbackUrl = Url.Page(
							"/Account/ConfirmEmail",
							pageHandler: null,
							values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
							protocol: Request.Scheme);

					string emailBody = $@"
                    <html>
                    <head>
                        <style>                           

                        </style>
                    </head>
                    <body>
                        <p>Olá,</p>
												<p>Agradecemos por se registar no <b>MasterBurger</b></p>
												<p>Estamos animados por tê-lo(a) a bordo para explorar todas as delícias que temos a oferecer.</p>
												<p>Para garantir que tenha acesso total à nossa plataforma e benefícios exclusivos, por favor, clique no link abaixo para confirmar a sua conta:</p>
												<div style='padding: 3px 20px; border-radius:5px; background-color: #E59E1D; width: 250px; text-align: center;'>
													 <p class='link'><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: black; text-decoration: none; text-transform: uppercase; font-weight: bold'>Confirmar Conta</a></p>                  
												</div>

												<br>
												<p>Se tiver alguma dúvida ou precisar de assistência, não hesite em entrar em contato conosco.</p>
												<br>
												<p>Atenciosamente,<br>Equipa MasterBurger</p>
                    </body>
                    </html>";

					await _emailSender.SendEmailAsync(Input.Email, "Confirme o seu endereço de e-mail", emailBody);

					if (_userManager.Options.SignIn.RequireConfirmedAccount) {
						return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
					} else {
						await _signInManager.SignInAsync(user, isPersistent: false);
						return LocalRedirect(returnUrl);
					}
				}
				foreach (var error in result.Errors) {
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return Page();

		}

		private IdentityUser CreateUser() {
			try {
				return Activator.CreateInstance<IdentityUser>();
			} catch {
				throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
						$"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
						$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
			}

		}

		private IUserEmailStore<IdentityUser> GetEmailStore() {
			if (!_userManager.SupportsUserEmail) {
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<IdentityUser>)_userStore;
		}
	}
}
