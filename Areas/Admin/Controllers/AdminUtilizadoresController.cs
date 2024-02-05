using MasterBurger.Areas.Admin.Views.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize]
	[Area("Admin")]
	[Route("Admin/AdminUtilizadores")]

	public class AdminUtilizadoresController : Controller {
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IEmailSender _emailSender;

		public AdminUtilizadoresController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender) {
			_userManager = userManager;
			_roleManager = roleManager;
			_emailSender = emailSender;
		}

		public IActionResult Index() {
			var users = _userManager.Users.ToList();
			return View(users);
		}


		[Route("EditarUserRoles")]
		public async Task<IActionResult> EditUserRoles(string userId) {
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) {
				return NotFound();
			}

			var userRoles = await _userManager.GetRolesAsync(user);
			var allRoles = _roleManager.Roles.ToList();

			var model = new EditRolesViewModels {
				UserId = userId,
				UserName = user.UserName,
				UserRoles = userRoles.ToList(),
				AllRoles = allRoles.Select(role => role.Name).ToList()
			};

			return View(model);
		}

		[HttpPost]
		[Route("EditarUserRoles")]
		public async Task<IActionResult> EditarUserRoles(EditRolesViewModels model) {
			var user = await _userManager.FindByIdAsync(model.UserId);
			if (user == null) {
				return NotFound();
			}

			var userRoles = await _userManager.GetRolesAsync(user);
			var selectedRole = model.SelectedRole; 

			var rolesToAdd = new List<string> { selectedRole }.Except(userRoles).ToList();
			var rolesToRemove = userRoles.Except(new List<string> { selectedRole }).ToList();

			await _userManager.AddToRolesAsync(user, rolesToAdd);
			await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

			return RedirectToAction("Index");
		}

		[Route("Create")]
		public IActionResult Create() {
			var allRoles = _roleManager.Roles.ToList();

			var model = new UtilizadorViewModel {
				AllRoles = allRoles.Select(role => role.Name).ToList()
			};

			return View(model);
		}

		[HttpPost]
		[Route("Create")]
		public async Task<IActionResult> Create(UtilizadorViewModel dados) {
			var allRoles = _roleManager.Roles.ToList();
			dados.AllRoles = allRoles.Select(role => role.Name).ToList();

			if (dados.Email != null) {
				var user = new IdentityUser { UserName = dados.Email, Email = dados.Email };

				var result = await _userManager.CreateAsync(user, dados.Password);
				if (result.Succeeded) {
					if (!string.IsNullOrEmpty(dados.SelectedRole)) {
						await _userManager.AddToRoleAsync(user, dados.SelectedRole);
					}

					// Enviea o email de confirmação
					var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
					code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
					var callbackUrl = Url.Action(
							"ConfirmEmail",
							"Account",
							new { area = "Identity", userId = user.Id, code },
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
                    </html>
					";

					await _emailSender.SendEmailAsync(dados.Email, "Confirme o seu endereço de e-mail", emailBody);

					return RedirectToAction("Index"); // Redireciona para a página de lista de utilizadores
				}

				foreach (var error in result.Errors) {
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return View(dados);
		}

	}
}
