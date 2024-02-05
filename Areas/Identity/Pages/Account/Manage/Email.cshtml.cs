// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MasterBurger.Areas.Identity.Pages.Account.Manage {
	public class EmailModel : PageModel {
		private readonly UserManager<IdentityUser> _userManager;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly IEmailSender _emailSender;

		public EmailModel(
				UserManager<IdentityUser> userManager,
				SignInManager<IdentityUser> signInManager,
				IEmailSender emailSender) {
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public bool IsEmailConfirmed { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string StatusMessage { get; set; }

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
		public class InputModel {
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			[Display(Name = "Novo email")]
			public string NewEmail { get; set; }
		}

		private async Task LoadAsync(IdentityUser user) {
			var email = await _userManager.GetEmailAsync(user);
			Email = email;

			Input = new InputModel {
				NewEmail = email,
			};

			IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
		}

		public async Task<IActionResult> OnGetAsync() {
			var user = await _userManager.GetUserAsync(User);
			if (user == null) {
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			await LoadAsync(user);
			return Page();
		}

		public async Task<IActionResult> OnPostChangeEmailAsync() {
			var user = await _userManager.GetUserAsync(User);
			if (user == null) {
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (!ModelState.IsValid) {
				await LoadAsync(user);
				return Page();
			}

			var email = await _userManager.GetEmailAsync(user);
			if (Input.NewEmail != email) {
				var userId = await _userManager.GetUserIdAsync(user);
				var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
						"/Account/ConfirmEmailChange",
						pageHandler: null,
						values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
						protocol: Request.Scheme);

				string emailBody = $@"
                    <html>
                    <head>
                        <style>                           

                        </style>
                    </head>
                    <body>
                        <p>Olá,</p>
												<p>Recebemos um pedido para alteração do endereço de e-mail associado à sua conta no <b>MasterBurger</b>. </p>
												<p>Por favor, clique no link abaixo para confirmar esta alteração:</p>
												<div style='padding: 3px 20px; border-radius:5px; background-color: #E59E1D; width: 250px; text-align: center;'>
													 <p class='link'><a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: black; text-decoration: none; text-transform: uppercase; font-weight: bold'>Confirmar Conta</a></p>                  
												</div>

												<br>
												<p>Se você não solicitou essa alteração, por favor, entre em contato conosco imediatamente.</p>
												<br>
												<p>Atenciosamente,<br>Equipa MasterBurger</p>
                    </body>
                    </html>";

				await _emailSender.SendEmailAsync(Input.NewEmail, "Confirme a alteração do seu endereço de e-mail", emailBody);

				StatusMessage = "Por favor, verifique a caixa de entrada do novo email para confirmar a alteração.";
				return RedirectToPage();
			}

			StatusMessage = "Não foi possível alterar o seu endereço de e-mail";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostSendVerificationEmailAsync() {
			var user = await _userManager.GetUserAsync(User);
			if (user == null) {
				return NotFound($"Não é possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
			}

			if (!ModelState.IsValid) {
				await LoadAsync(user);
				return Page();
			}

			var userId = await _userManager.GetUserIdAsync(user);
			var email = await _userManager.GetEmailAsync(user);
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

			await _emailSender.SendEmailAsync(email, "Confirme o seu endereço de e-mail", emailBody);

			StatusMessage = "Email de verificação enviado. Verifique o sua caixa de entrada.";
			return RedirectToPage();
		}
	}
}
