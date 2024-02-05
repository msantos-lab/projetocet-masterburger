using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Models {
  [Table("CarrinhoCompraItens")]

  public class CarrinhoCompraItem {
    [Key]
    public int CarrinhoCompraItemId { get; set; }

    public int Quantidade { get; set; }

    [StringLength(200)]
    public string CarrinhoCompraId { get; set; }

    public Produto Produto { get; set; }
	}
}
