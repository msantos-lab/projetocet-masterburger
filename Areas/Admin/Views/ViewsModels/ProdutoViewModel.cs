using MasterBurger.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Areas.Admin.Views.ViewsModels {
  public class ProdutoViewModel {

    [Display(Name = "Nome do produto")]
    [Column(TypeName = "varchar(80)")]
    public string Nome { get; set; }

    public int Quantidade { get; set; } = 0;

    [Display(Name = "Nome do produto")]
    [Column(TypeName = "varchar(200)")]
    public string Autor { get; set; }

    [Display(Name = "Descrição curta do produto")]
    [Column(TypeName = "varchar(95)")]
    public string DescricaoCurta { get; set; }

    [Display(Name = "Sinopse do produto")]
    [Column(TypeName = "varchar(Max)")]
    public string DescricaoDetalhada { get; set; }

    [Display(Name = "Detalhe do produto")]
    [Column(TypeName = "varchar(Max)")]
    public string Detalhe { get; set; }

    [Display(Name = "Preço")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; }

    [Display(Name = "Preço Anterior")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoAnterior { get; set; }

    [Display(Name = "ImagemBase64")]
    [Column(TypeName = "varbinary(MAX)")]
    public byte[] ImagemBase64 { get; set; }

    [NotMapped]
    public IFormFile ImagemProduto { get; set; }

    [Display(Name = "Produto em destaque na home?")]
    public bool IsDestaque { get; set; }

    [Display(Name = "Produto disponível?")]
    public bool IsDisponivel { get; set; }

    public int CategoriaId { get; set; }

    public int BannerId { get; set; } = 0;

		public virtual ICollection<ProdutosRelacionados> ProdRelacionados { get; set; }

	}
}
