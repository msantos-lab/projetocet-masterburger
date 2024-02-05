// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MasterBurger.Areas.Identity.Pages.Account {
	[AllowAnonymous]
	public class ResendEmailConfirmationModel : PageModel {
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmailSender _emailSender;

		public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender) {
			_userManager = userManager;
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
		public class InputModel {
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }
		}

		public void OnGet() {
		}

		public async Task<IActionResult> OnPostAsync() {
			if (!ModelState.IsValid) {
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(Input.Email);
			if (user == null) {
				ModelState.AddModelError(string.Empty, "Email de verificação enviado. Verifique o sua caixa de entrada.");
				return Page();
			}

			var userId = await _userManager.GetUserIdAsync(user);
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { userId = userId, code = code },
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

			ModelState.AddModelError(string.Empty, "Email de verificação enviado. Verifique o sua caixa de entrada.");
			return Page();
		}
	}
}
