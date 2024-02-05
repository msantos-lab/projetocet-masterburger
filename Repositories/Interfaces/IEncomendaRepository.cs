using MasterBurger.Models;

namespace MasterBurger.Repositories.Interfaces {
  public interface IEncomendaRepository {
    void CriarEncomenda(Encomenda Encomenda);
    string ObterMaiorEncomendaId();

  }
}
