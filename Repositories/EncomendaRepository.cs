using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Repositories {
	public class EncomendaRepository : IEncomendaRepository {

    private readonly ApplicationDbContext _context;
    private readonly CarrinhoCompra _carrinho;

    public EncomendaRepository(ApplicationDbContext appDbContext,
        CarrinhoCompra carrinho) {
      _context = appDbContext;
      _carrinho = carrinho;
    }

		public void CriarEncomenda(Encomenda Encomenda) {
			Encomenda.EncomendaRealizada = DateTime.Now;

			_context.Encomendas.Add(Encomenda);
			_context.SaveChanges();

			var carrinhoCompraItens = _carrinho.CarrinhoCompraItems;

			foreach (var carrinhoItem in carrinhoCompraItens) {
				var EncomendaDetail = new EncomendaDetalhe() {
					Quantidade = (int)carrinhoItem.Quantidade,
					ProdutoId = carrinhoItem.Produto.ProdutoId,
					EncomendaId = Encomenda.EncomendaId,
					Preco = carrinhoItem.Produto.Preco
				};

				// Atualiza a quantidade em estoque do produto
				var produto = _context.Produtos.Find(carrinhoItem.Produto.ProdutoId);
				if (produto != null) {
					produto.Quantidade -= (int)carrinhoItem.Quantidade;

					// Verifica se a quantidade do produto é zero
					if (produto.Quantidade == 0) {
						produto.IsDisponivel = false;
					}

					_context.Entry(produto).State = EntityState.Modified;
				}

				_context.EncomendaDetalhes.Add(EncomendaDetail);
			}

			_context.SaveChanges();
		}


		public string ObterMaiorEncomendaId() {
      string maiorEncomendaId = _context.Encomendas.Max(p => p.EncomendaId);
      return maiorEncomendaId;
    }
  }
}
