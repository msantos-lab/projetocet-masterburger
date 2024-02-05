using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Controllers {
	[Authorize]
	[Route("customerdados")]
	public class CustomerDadosController : Controller {

		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _context;

		public CustomerDadosController(ApplicationDbContext context, UserManager<IdentityUser> userManager) {
			_context = context;
			_userManager = userManager;
		}


		public IActionResult Index() {
			// Verifica se há dados registrados na tabela DadosUtilizador
			var userTask = _userManager.GetUserAsync(User);

			userTask.Wait();

			var user = userTask.Result;

			bool hasData = _context.DadosUser.Any(d => d.UserId == user.Id.ToString());

			if (hasData) {
				return RedirectToAction("DadosListar");
			} else {
				return RedirectToAction("DadosRegistar");
			}
		}


		[HttpGet]
		[Route("dadospessoais")]
		public async Task<IActionResult> DadosListar(DadosUtilizador dadosUtilizador) {

			IList<DadosUtilizador> DadosUtilizador = new List<DadosUtilizador>();

			var user = await _userManager.GetUserAsync(User);

			if (user != null) {

				DadosUtilizador = _context.DadosUser
							 .Where(dadosUser => dadosUser.UserId == user.Id)
							 .SelectMany(dadosUser => _context.DadosUtilizador
									 .Where(dadosUtilizador => dadosUtilizador.DadosUtilizadorId == dadosUser.DadosUtilId))
							 .ToList();
			}

			return View(DadosUtilizador);
		}

		[HttpGet]
		[Route("dadosregistar")]
		public IActionResult DadosRegistar() {
			return View();
		}

		[HttpPost]
		[Route("dadosregistar")]
		public async Task<IActionResult> DadosRegistar([FromForm] string NIF) {
			var userId = _userManager.GetUserId(User);
			var user = await _userManager.GetUserAsync(User);

			if (_context.DadosUser.Any(du => du.UserId == userId)) {
				ViewData["Mensagem"] = "Você já possui uma morada registada.";
				return View();
			}

			if (ModelState.IsValid) {
				string dadosUtilizadorId = Guid.NewGuid().ToString();

				DadosUtilizador dadosUtilizador = new DadosUtilizador {
					DadosUtilizadorId = dadosUtilizadorId,
					IdUser = userId,
					Email = user.Email,
					Nome = Request.Form["Nome"],
					Apelido = Request.Form["Apelido"],
					NIF = Request.Form["NIF"],
					Morada = Request.Form["Morada"],
					Localidade = Request.Form["Localidade"],
					CodigoPostal = Request.Form["CodigoPostal"],
					Telemovel = Request.Form["Telemovel"]
				};

				_context.DadosUtilizador.Add(dadosUtilizador);
				_context.SaveChanges();

				DadosUser dadosUser = new DadosUser {
					UserId = userId,
					DadosUtilId = dadosUtilizadorId
				};

				_context.DadosUser.Add(dadosUser);
				_context.SaveChanges();

				// Atualiza as informações personalizadas do utilizador
				await _userManager.UpdateAsync(user);

				return RedirectToAction("DadosListar");
			}

			return View("Index");
		}



		[HttpGet]
		[Route("dadoseditar")]
		public async Task<IActionResult> DadosEditar(string id) {
			var userId = _userManager.GetUserId(User);
			var user = await _userManager.GetUserAsync(User);

			var dados = await _context.DadosUser
					.Where(du => du.UserId == userId && du.DadosUtilId == id)
					.Select(du => _context.DadosUtilizador.FirstOrDefault(duu => duu.DadosUtilizadorId == du.DadosUtilId))
					.FirstOrDefaultAsync();

			if (dados == null) {
				return RedirectToAction("DadosRegistar");
			}

			var model = new DadosUtilizador {
				IdUser = userId,
				Email = user.Email,
				Nome = dados.Nome,
				Apelido = dados.Apelido,
				NIF = dados.NIF,
				Morada = dados.Morada,
				Localidade = dados.Localidade,
				CodigoPostal = dados.CodigoPostal,
				Telemovel = dados.Telemovel
			};

			return View(model);
		}

		[HttpPost]
		[Route("dadoseditar")]
		public async Task<IActionResult> DadosEditar(DadosUtilizador model) {
			if (ModelState.IsValid) {
				var userId = _userManager.GetUserId(User);
				var user = await _userManager.GetUserAsync(User);

				var dados = await _context.DadosUser
			.Where(du => du.UserId == userId)
			.Select(du => _context.DadosUtilizador.FirstOrDefault(duu => duu.DadosUtilizadorId == du.DadosUtilId))
			.FirstOrDefaultAsync();


				if (dados == null) {
					return RedirectToAction("DadosRegistar");
				}

				dados.IdUser = userId;
				dados.Email = user.Email;
				dados.Nome = model.Nome;
				dados.Apelido = model.Apelido;
				dados.NIF = model.NIF;
				dados.Morada = model.Morada;
				dados.Localidade = model.Localidade;
				dados.CodigoPostal = model.CodigoPostal;
				dados.Telemovel = model.Telemovel;

				_context.SaveChanges();

				return RedirectToAction("DadosListar");
			}

			return View(model);
		}

	}
}
