using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize(Roles = "Cozinha,Admin")]
	[Area("Admin")]
	public class AdminCozinhaController : Controller {
		private readonly ApplicationDbContext _context;

		public AdminCozinhaController(ApplicationDbContext context) {
			_context = context;
		}

		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "EncomendaId") {
			// Filtra as encomendas com status da Cozinha
			var encomendas = _context.Encomendas
					.Where(e => e.Status == "Encomenda Recebida" || e.Status == "Em Preparo" || e.Status == "Pronto para Entrega");


			if (!string.IsNullOrWhiteSpace(filter)) {
				encomendas = encomendas.Where(p => p.Status.Contains(filter));
			}

			var model = await PagingList.CreateAsync(encomendas, 10, pageindex, sort, "EncomendaId");
			model.RouteValue = new RouteValueDictionary { { "filter", filter } };
			return View(model);
		}


		[Route("Admin/AdminCozinha/Details")]
		public IActionResult Details(string id) {
			if (id == null) {
				return NotFound();
			}

			// Carrega a encomenda com os detalhes dos produtos associados
			var encomenda = _context.Encomendas
					.Include(e => e.EncomendaItens) 
					.ThenInclude(ed => ed.Produto)  
					.FirstOrDefault(e => e.EncomendaId == id);


			if (encomenda == null) {
				return NotFound();
			}

			return View(encomenda);
		}


		[Route("Admin/AdminCozinha/AlterarStatusEncomenda")]
		public async Task<IActionResult> AlterarStatusEncomenda(string id, string status) {
			if (id == null) {
				return NotFound();
			}

			var Encomenda = await _context.Encomendas.FindAsync(id);

			if (Encomenda == null) {
				return NotFound();
			}

			try {
				Encomenda.Status = status;

				_context.Encomendas.Update(Encomenda);
				await _context.SaveChangesAsync();
			} catch (Exception ex) {
				Console.WriteLine($"Erro ao alterar o status da encomenda: {ex.Message}");
			}

			return RedirectToAction("Details", new { id = Encomenda.EncomendaId });
		}

	}
}
