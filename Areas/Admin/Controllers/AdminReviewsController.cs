using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {

	[Authorize]
	[Area("Admin")]
	public class AdminReviewsController : Controller {
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public AdminReviewsController(ApplicationDbContext context, UserManager<IdentityUser> userManager) {
			_context = context;
			_userManager = userManager;
		}


		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "-ReviewId") {
			var resultado = _context.ProdutoReviews.Include(l => l.Produto).AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter)) {
				resultado = resultado.Where(p => p.Status.Contains(filter));
			}

			var model = await PagingList.CreateAsync(resultado, 5, pageindex, sort, "ReviewId");
			model.RouteValue = new RouteValueDictionary { { "filter", filter } };
			return View(model);
		}

		public async Task<IActionResult> Details(int id) {
			var reviewDetails = await _context.ProdutoReviews
					.FirstOrDefaultAsync(r => r.ReviewId == id);

			if (reviewDetails == null) {
				return NotFound();
			}

			return View(reviewDetails);
		}


		public async Task<IActionResult> Edit(int? id) {
			if (id == null || _context.ProdutoReviews == null) {
				return NotFound();
			}

			var review = await _context.ProdutoReviews.FindAsync(id);
			if (review == null) {
				return NotFound();
			}
			return View(review);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Status")] ProdutoReview produtoReview) {
			if (id != produtoReview.ReviewId) {
				return NotFound();
			}

			if (id == produtoReview.ReviewId) {
				try {
					var existingReview = await _context.ProdutoReviews.FindAsync(id);

					if (existingReview == null) {
						return NotFound();
					}

					// Altera apenas o Status
					existingReview.Status = produtoReview.Status;

					_context.Update(existingReview);
					await _context.SaveChangesAsync();
				} catch (DbUpdateConcurrencyException) {
					if (!ProdutoReviewExists(produtoReview.ReviewId)) {
						return NotFound();
					} else {
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}

			return View(produtoReview);
		}


		[HttpGet]
		public async Task<IActionResult> NotificarCliente(int id) {
			var review = await _context.ProdutoReviews.FindAsync(id);

			if (review == null) {
				return NotFound();
			}

			// Perfis dos admins
			var admins = await _userManager.GetUsersInRoleAsync("Admin");

			if (review.Status != "Nova") {
				// Configura o e-mail
				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("MasterBurger", "monica.santos.24244@formandos.cinel.pt"));
				message.To.Add(new MailboxAddress("", review.ClienteEmail));
				message.Subject = "Atualização Status da Avaliação - MasterBurger";

				// Envia o email para todos os admins
				foreach (var admin in admins) {
					message.Bcc.Add(new MailboxAddress("", admin.Email));
				}

				var builder = new BodyBuilder();
				if (review.Status == "Aprovada") {
					builder.HtmlBody = $"Prezado(a) {review.ClienteNome},<br><br>A avaliação do produto recebeu uma atualização de status para 'Aprovada' e será exibida em nosso site.<br>Agradecemos pela sua avaliação!<br><br>Atenciosamente,<br>MasterBurger";
				} else {
					builder.HtmlBody = $"Prezado(a) {review.ClienteNome},<br><br>A revisão do produto recebeu uma atualização de status para 'Reprovada' e não será exibida em nosso site, pois infringe  os nossos <a href='https://localhost:7178/termos-condicoes-avaliacoes' target='_blank'>Termos e Condições das Avaliações</a>.<br> <br><br>Atenciosamente,<br>MasterBurger";
				}

				message.Body = builder.ToMessageBody();

				try {
					using (var client = new MailKit.Net.Smtp.SmtpClient()) {
						client.Connect("smtp.office365.com", 587, false);
						client.Authenticate("monica.santos.24244@formandos.cinel.pt", "301013Fm");
						client.Send(message);
						client.Disconnect(true);

						Console.WriteLine("E-mail enviado com sucesso!");
						TempData["Mensagem"] = "E-mail enviado com sucesso!";
					}
				} catch (Exception ex) {
					Console.WriteLine("Ocorreu um erro ao enviar o e-mail: " + ex.Message);
					TempData["Mensagem"] = "Erro ao enviar o e-mail.";
				}
			}

			await AlterarStatusNotificacaoCliente(id);

			return RedirectToAction(nameof(Details), new { id });
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AlterarStatusNotificacaoCliente(int id) {
			var review = await _context.ProdutoReviews.FindAsync(id);

			if (review == null) {
				return NotFound();
			}

			try {
				// Altera apenas a coluna StatusNotCliente
				review.StatusNotCliente = true;

				_context.Update(review);
				await _context.SaveChangesAsync();
			} catch (DbUpdateConcurrencyException) {
				if (!ProdutoReviewExists(review.ReviewId)) {
					return NotFound();
				} else {
					throw;
				}
			}

			return RedirectToAction(nameof(Details), new { id });
		}

		private bool ProdutoReviewExists(int id) {
			return _context.ProdutoReviews.Any(e => e.ReviewId == id);
		}

	}
}
