using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;

namespace MasterBurger.Repositories {
	public class CupomRepository : ICupomRepository {

		private readonly ApplicationDbContext _context;

		public CupomRepository(ApplicationDbContext context) {
			_context = context;
		}

		public IEnumerable<Cupom> Cupons => _context.Cupons;

		public Cupom ObterCupomPorCodigo(string codigo) {
			return _context.Cupons.FirstOrDefault(c => c.Codigo == codigo);
		}

	}
}

