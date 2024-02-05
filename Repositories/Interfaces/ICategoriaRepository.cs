using MasterBurger.Models;

namespace MasterBurger.Repositories.Interfaces {
  public interface ICategoriaRepository {
    IEnumerable<Categoria> Categorias { get; }
  }
}
