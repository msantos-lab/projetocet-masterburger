using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize]
  [Area("Admin")]
  public class AdminCategoriasController : Controller {
    private readonly ApplicationDbContext _context;

    public AdminCategoriasController(ApplicationDbContext context) {
      _context = context;
    }

    public async Task<IActionResult> Index() {
      return View(await _context.Categorias.ToListAsync());
    }

    public IActionResult Create() {
      return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CategoriaId,CategoriaNome,CategoriaDescricao,CategoriaTipo")] Categoria categoria) {
      if (ModelState.IsValid) {
        _context.Add(categoria);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(categoria);
    }

    public async Task<IActionResult> Edit(int? id) {
      if (id == null) {
        return NotFound();
      }

      var categoria = await _context.Categorias.FindAsync(id);
      if (categoria == null) {
        return NotFound();
      }
      return View(categoria);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,CategoriaNome,CategoriaDescricao,CategoriaTipo")] Categoria categoria) {
      if (id != categoria.CategoriaId) {
        return NotFound();
      }

      if (ModelState.IsValid) {
        try {
          _context.Update(categoria);
          await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException) {
          if (!CategoriaExists(categoria.CategoriaId)) {
            return NotFound();
          } else {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }
      return View(categoria);
    }

    public async Task<IActionResult> Delete(int? id) {
      if (id == null) {
        return NotFound();
      }

      var categoria = await _context.Categorias
          .FirstOrDefaultAsync(m => m.CategoriaId == id);
      if (categoria == null) {
        return NotFound();
      }

      return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id) {
      var categoria = await _context.Categorias.FindAsync(id);

      if (categoria == null) {
        return NotFound();
      }

      // Verifica se existem produtos associados a esta categoria
      bool categoriaTemProdutos = _context.Produtos.Any(p => p.CategoriaId == categoria.CategoriaId);

      if (categoriaTemProdutos) {
        // Se houver produtos associados, retorna uma mensagem de erro
        var errorModel = new ErrorCategoriaViewModel {
          Mensagem = "Esta categoria está associada a produtos e não pode ser excluída."
        };

        return View("Error", errorModel);
      }

      _context.Categorias.Remove(categoria);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    private bool CategoriaExists(int id) {
      return _context.Categorias.Any(e => e.CategoriaId == id);
    }
  }
}
