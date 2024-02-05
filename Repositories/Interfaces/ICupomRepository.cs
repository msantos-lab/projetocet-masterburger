using MasterBurger.Models;

namespace MasterBurger.Repositories.Interfaces {
  public interface ICupomRepository {
		IEnumerable<Cupom> Cupons { get; }

		Cupom ObterCupomPorCodigo(string codigo);

	}
}
