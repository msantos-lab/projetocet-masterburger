using MasterBurger.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MasterBurger.Components {
  public class CategoriaMenu : ViewComponent {
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaMenu(ICategoriaRepository categoriaRepository) {
      _categoriaRepository = categoriaRepository;
    }

    public IViewComponentResult Invoke() {
      var categorias = _categoriaRepository.Categorias.OrderBy(c => c.CategoriaTipo).ThenBy(c => c.CategoriaNome);
      return View(categorias);
    }
  }
}
