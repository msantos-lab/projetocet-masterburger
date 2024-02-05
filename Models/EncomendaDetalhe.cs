using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {
  public class EncomendaDetalhe {
    public int EncomendaDetalheId { get; set; }

    public string EncomendaId { get; set; }

    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Preco { get; set; }

    public virtual Produto Produto { get; set; }

    public virtual Encomenda Encomenda { get; set; }
  }
}
