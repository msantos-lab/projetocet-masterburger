// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Drawing;
using SixLabors.ImageSharp;

namespace MasterBurger.Areas.Identity.Pages.Account {
	[AllowAnonymous]
	public class ExternalLoginModel : PageModel {
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IUserStore<IdentityUser> _userStore;
		private readonly IUserEmailStore<IdentityUser> _emailStore;
		private readonly IEmailSender _emailSender;
		private readonly ILogger<ExternalLoginModel> _logger;

		public ExternalLoginModel(
				SignInManager<IdentityUser> signInManager,
				UserManager<IdentityUser> userManager,
				IUserStore<IdentityUser> userStore,
				ILogger<ExternalLoginModel> logger,
				IEmailSender emailSender) {
			_signInManager = signInManager;
			_userManager = userManager;
			_userStore = userStore;
			_emailStore = GetEmailStore();
			_logger = logger;
			_emailSender = emailSender;
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
		public string ProviderDisplayName { get; set; }

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
		}

		public IActionResult OnGet() => RedirectToPage("./Login");

		public IActionResult OnPost(string provider, string returnUrl = null) {
			// Request a redirect to the external login provider.
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null) {
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null) {
				ErrorMessage = $"Error from external provider: {remoteError}";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null) {
				ErrorMessage = "Error loading external login information.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			// Sign in the user with this external login provider if the user already has a login.
			var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			if (result.Succeeded) {
				_logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

				// Role "Cliente"
				var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (user != null) {
					if (!await _userManager.IsInRoleAsync(user, "Cliente")) {
						await _userManager.AddToRoleAsync(user, "Cliente");
					}
					ViewData["Status"] = "Success";
					return LocalRedirect(returnUrl);
				}
			} else if (result.IsLockedOut) {
				return RedirectToPage("./Lockout");
			}

			// If the user does not have an account, then ask the user to create an account.
			ReturnUrl = returnUrl;
			ProviderDisplayName = info.ProviderDisplayName;
			if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email)) {
				Input = new InputModel {
					Email = info.Principal.FindFirstValue(ClaimTypes.Email)
				};

				// Attempt to create a new user and send confirmation email
				var user = await _userManager.FindByEmailAsync(Input.Email);
				if (user == null) {
					user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
					var createResult = await _userManager.CreateAsync(user);

					if (createResult.Succeeded) {
						await _userManager.AddLoginAsync(user, info);
						await _userManager.AddToRoleAsync(user, "Cliente"); 

						var userId = await _userManager.GetUserIdAsync(user);
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

						ViewData["Status"] = "Success";
						return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
					} else {
						foreach (var error in createResult.Errors) {
							ModelState.AddModelError(string.Empty, error.Description);
						}
						return Page();
					}
				} else {
					if (!user.EmailConfirmed) {
						// O e-mail ainda não foi confirmado
						ViewData["Status"] = "PendingConfirmation";
						ModelState.AddModelError(string.Empty, "Por favor, verifique a caixa de entrada do email abaixo para confirmar a sua conta.");
						return Page();
					}

					await _userManager.AddLoginAsync(user, info);

					return LocalRedirect(returnUrl);
				}
			}

			ViewData["Status"] = "EmailAlreadyRegistered";
			ModelState.AddModelError(string.Empty, $"Email '{Input.Email}' já registado.");


			return Page();
		}


		public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null) {
			returnUrl = returnUrl ?? Url.Content("~/");
			// Get the information about the user from the external login provider
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null) {
				ErrorMessage = "Error loading external login information during confirmation.";
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}

			if (ModelState.IsValid) {
				var user = CreateUser();

				await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
				await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded) {
					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded) {
						_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

						var userId = await _userManager.GetUserIdAsync(user);
						var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
						code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
						var callbackUrl = Url.Page(
								"/Account/ConfirmEmail",
								pageHandler: null,
								values: new { area = "Identity", userId = userId, code = code },
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

						// If account confirmation is required, we need to show the link if we don't have a real email sender
						if (_userManager.Options.SignIn.RequireConfirmedAccount) {
							return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
						}

						await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						return LocalRedirect(returnUrl);
					}
				}
				foreach (var error in result.Errors) {
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			ProviderDisplayName = info.ProviderDisplayName;
			ReturnUrl = returnUrl;
			return Page();
		}

		private IdentityUser CreateUser() {
			try {
				return Activator.CreateInstance<IdentityUser>();
			} catch {
				throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
						$"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
						$"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
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
