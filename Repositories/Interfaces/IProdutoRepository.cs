using MasterBurger.Models;

namespace MasterBurger.Repositories.Interfaces {
  public interface IProdutoRepository {
    IEnumerable<Produto> Produtos { get; }

    IEnumerable<Produto> ProdutosPreferidos { get; }

    Produto GetProdutoById(int produtoId);

    void AtualizarQuantidadeProduto(int produtoId, int quantidadeVendida);
  }
}
