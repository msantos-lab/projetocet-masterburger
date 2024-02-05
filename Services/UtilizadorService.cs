using MasterBurger.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace MasterBurger.Services {
	public class UtilizadorService {
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _context;

		public UtilizadorService(UserManager<IdentityUser> userManager, ApplicationDbContext context) {
			_userManager = userManager;
			_context = context;
		}

		public async Task<string> GetNomeAsync(ClaimsPrincipal user, string userId) {
			var dadosUtilId = await _context.DadosUser
					.Where(d => d.UserId == userId)
					.Select(d => d.DadosUtilId)
					.FirstOrDefaultAsync();

			if (dadosUtilId == null || dadosUtilId == "0") {
				string userName = user.Identity.Name;

				if (string.IsNullOrEmpty(userName)) {
					return "Utilizador não encontrado";
				}

				return userName;
			}

			var dadosUtilizador = await _context.DadosUtilizador
					.Where(d => d.DadosUtilizadorId == dadosUtilId)
					.FirstOrDefaultAsync();

			if (dadosUtilizador == null) {
				return "Utilizador não encontrado";
			}

			return dadosUtilizador.Nome;
		}

	}
}
