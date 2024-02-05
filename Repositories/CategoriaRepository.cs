using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;

namespace MasterBurger.Repositories {
  public class CategoriaRepository : ICategoriaRepository {

    private readonly ApplicationDbContext _context;

    public CategoriaRepository(ApplicationDbContext context) {
      _context = context;
    }

    public IEnumerable<Categoria> Categorias => _context.Categorias;
  }
}
