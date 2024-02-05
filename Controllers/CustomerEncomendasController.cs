using MasterBurger.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Controllers {
	[Authorize]
	public class CustomerEncomendasController : Controller {

		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _context;

		public CustomerEncomendasController(ApplicationDbContext context, UserManager<IdentityUser> userManager) {
			_context = context;
			_userManager = userManager;
		}

		public IActionResult Index() {
      var userTask = _userManager.GetUserAsync(User); 
      userTask.Wait(); 

      var user = userTask.Result; 

      if (user != null) {
        var encomendas = _context.Encomendas.Where(e => e.UserId == user.Id.ToString()).ToList();
        return View(encomendas);
      }

      return View();
    }

		public IActionResult Detail(string id) {
      var Encomenda = _context.Encomendas.Include(p => p.EncomendaItens).ThenInclude(d => d.Produto).FirstOrDefault(p => p.EncomendaId == id);

      if (Encomenda == null) {
				return NotFound(); 
			}

			return View(Encomenda);
		}
	}
}
