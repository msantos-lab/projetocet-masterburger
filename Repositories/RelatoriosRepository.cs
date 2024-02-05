using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Repositories {
	public class RelatoriosRepository : IRelatoriosRepository {
		private readonly ApplicationDbContext _dbContext;

		public RelatoriosRepository(ApplicationDbContext dbContext) {
			_dbContext = dbContext;
		}

		public List<VendasTotaisViewModel> ObterVendasTotais(DateTime dataInicio, DateTime dataFim) {
			var vendasTotais = _dbContext.Encomendas
					.Where(e => e.EncomendaRealizada >= dataInicio && e.EncomendaRealizada <= dataFim)
					.GroupBy(e => e.EncomendaRealizada.Date)
					.Select(g => new VendasTotaisViewModel {
						DataVenda = g.Key,
						ValorTotal = g.Sum(e => e.EncomendaTotal)
					})
					.ToList();

			return vendasTotais;
		}

		public List<VendasPorCategoriaViewModel> ObterVendasPorCategoria(DateTime dataInicio, DateTime dataFim) {
			var vendasPorCategoria = _dbContext.Encomendas
					.Where(e => e.EncomendaRealizada >= dataInicio && e.EncomendaRealizada <= dataFim)
					.Join(_dbContext.EncomendaDetalhes, encomenda => encomenda.EncomendaId, detalhe => detalhe.EncomendaId, (encomenda, detalhe) => new { encomenda, detalhe })
					.Where(result => result.encomenda.EncomendaRealizada >= dataInicio && result.encomenda.EncomendaRealizada <= dataFim)
					.GroupBy(result => result.detalhe.Produto.Categoria.CategoriaNome)
					.Select(g => new VendasPorCategoriaViewModel {
						Categoria = g.Key,
						ValorTotal = g.Sum(result => result.detalhe.Preco * result.detalhe.Quantidade)
					})
					.Take(5)
					.ToList();

			return vendasPorCategoria;
		}


		public List<ProdutosMaisVendidosViewModel> ObterProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim) {
			var produtosMaisVendidos = _dbContext.EncomendaDetalhes
					.Where(d => d.Encomenda.EncomendaRealizada >= dataInicio && d.Encomenda.EncomendaRealizada <= dataFim)
					.GroupBy(d => d.Produto.Nome)
					.Select(g => new ProdutosMaisVendidosViewModel {
						NomeProduto = g.Key,
						QuantidadeVendida = g.Sum(d => d.Quantidade)
					})
					.OrderByDescending(p => p.QuantidadeVendida)
					.Take(5)
					.ToList();

			return produtosMaisVendidos;
		}

		public List<object> RelatorioVendasTotais(DateTime dataInicio, DateTime dataFim) {
      var vendasTotais = _dbContext.Encomendas
          .Include(e => e.EncomendaItens)
          .Where(e => e.EncomendaRealizada >= dataInicio && e.EncomendaRealizada <= dataFim)
          .SelectMany(e => e.EncomendaItens.Select(d => new {
            EncomendaId = e.EncomendaId,
            DataVenda = e.EncomendaRealizada.Date.ToString("dd-MM-yyyy"),
            NomeProduto = d.Produto.Nome,
            Quantidade = d.Quantidade,
            PrecoUnitario = d.Preco,
            ValorTotalVendido = d.Quantidade * d.Preco 
          }))
          .ToList();

      var vendasTotaisMap = vendasTotais.Cast<object>().ToList();
			return vendasTotaisMap;
		}

		public List<object> RelatorioVendasPorCategoria(DateTime dataInicio, DateTime dataFim) {
			var vendasPorCategoria = _dbContext.Encomendas
					.Where(e => e.EncomendaRealizada >= dataInicio && e.EncomendaRealizada <= dataFim)
					.Join(_dbContext.EncomendaDetalhes, encomenda => encomenda.EncomendaId, detalhe => detalhe.EncomendaId, (encomenda, detalhe) => new { encomenda, detalhe })
					.GroupBy(result => result.detalhe.Produto.Categoria.CategoriaNome)
					.Select(g => new VendasPorCategoriaViewModel {
						Categoria = g.Key,
						ValorTotal = g.Sum(result => result.detalhe.Preco * result.detalhe.Quantidade)
					})
					.ToList();

			var vendasPorCategoriaMap = vendasPorCategoria.Cast<object>().ToList();
			return vendasPorCategoriaMap;
		}

		public List<object> RelatorioProdutosMaisVendidos(DateTime dataInicio, DateTime dataFim) {
			var produtosMaisVendidos = _dbContext.EncomendaDetalhes
					.Where(d => d.Encomenda.EncomendaRealizada >= dataInicio && d.Encomenda.EncomendaRealizada <= dataFim)
					.GroupBy(d => d.Produto.Nome)
					.Select(g => new ProdutosMaisVendidosViewModel {
						NomeProduto = g.Key,
						QuantidadeVendida = g.Sum(d => d.Quantidade)
					})
					.OrderByDescending(p => p.QuantidadeVendida)
					.ToList();

			var produtosMaisVendidosMap = produtosMaisVendidos.Cast<object>().ToList();
			return produtosMaisVendidosMap;
		}

	}
}


