using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {

	[Authorize]
	[Area("Admin")]
	public class AdminCuponsController : Controller {

		private readonly ApplicationDbContext _context;

		public AdminCuponsController(ApplicationDbContext context) {
			_context = context;
		}

		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "CupomId") {
			var cupons = _context.Cupons.AsQueryable();

			var model = await PagingList.CreateAsync(cupons, 10, pageindex, sort, "CupomId");
			return View(model);
		}



		// GET: AdminCuponsController/Create
		public ActionResult Create() {
			return View();
		}

		// POST: AdminCuponsController/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Cupom cupom) {
			if (ModelState.IsValid) {
				_context.Add(cupom);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(cupom);
		}


		// GET: AdminCuponsController/Edit/5
		public async Task<IActionResult> Edit(int? id) {
			if (id == null) {
				return NotFound();
			}

			var cupom = await _context.Cupons.FindAsync(id);
			if (cupom == null) {
				return NotFound();
			}
			return View(cupom);
		}

		// POST: AdminCuponsController/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Cupom cupom) {
			if (id != cupom.CupomId) {
				return NotFound();
			}

			if (ModelState.IsValid) {
				try {
					_context.Update(cupom);
					await _context.SaveChangesAsync();
				} catch (DbUpdateConcurrencyException) {
					if (!CupomExists(cupom.CupomId)) {
						return NotFound();
					} else {
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(cupom);
		}

		private bool CupomExists(int id) {
			return _context.Cupons.Any(e => e.CupomId == id);
		}
	}
}
