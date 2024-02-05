using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Repositories {
  public class ProdutoRepository : IProdutoRepository {

    private readonly ApplicationDbContext _context;

    public ProdutoRepository(ApplicationDbContext context) {
      _context = context;
    }

		public IEnumerable<Produto> Produtos => _context.Produtos.Include(c => c.Categoria).Include(p => p.Reviews);


		public IEnumerable<Produto> ProdutosPreferidos {
      get {
        if (_context != null) {
					return _context.Produtos.Where(p => p.IsDestaque).Include(c => c.Categoria).Include(p => p.Reviews);
				} else {
          return Enumerable.Empty<Produto>(); // Retorna uma coleção vazia
        }
      }
    }

    public Produto GetProdutoById(int produtoId) {
      return _context.Produtos.FirstOrDefault(l => l.ProdutoId == produtoId);
    }

    public void AtualizarQuantidadeProduto(int produtoId, int quantidadeVendida) {
      var produto = _context.Produtos.Find(produtoId);

      if (produto != null) {
        produto.Quantidade = Math.Max(0, produto.Quantidade - quantidadeVendida);

        // Se a quantidade atingir 0, altera IsDisponivel para false
        if (produto.Quantidade == 0) {
          produto.IsDisponivel = false;
        }

        _context.SaveChanges();
      }
    }
  }
}
