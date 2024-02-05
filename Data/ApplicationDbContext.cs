using MasterBurger.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Data {
  public class ApplicationDbContext : IdentityDbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) {
    }
    public DbSet<DadosUtilizador> DadosUtilizador { get; set; }

    public DbSet<DadosUser> DadosUser { get; set; }

    public DbSet<Categoria> Categorias { get; set; }

    public DbSet<Produto> Produtos { get; set; }

    public DbSet<Encomenda> Encomendas { get; set; }

    public DbSet<EncomendaDetalhe> EncomendaDetalhes { get; set; }

		public DbSet<Banner> Banners { get; set; }

		public DbSet<CarrinhoCompraItem> CarrinhoCompraItens { get; set; }

		public DbSet<ProdutoReview> ProdutoReviews { get; set; }

		public DbSet<ProdutosRelacionados> ProdutosRelacionados { get; set; }

		public DbSet<Cupom> Cupons { get; set; }
	}
}