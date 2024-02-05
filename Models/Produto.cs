using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {

  [Table("Produtos")]
  public class Produto {
    [Key]
    public int ProdutoId { get; set; }

    public int Quantidade { get; set; }

    [Display(Name = "Nome do produto")]
    public string Nome { get; set; }

    [Display(Name = "Descrição curta")]
    [Column(TypeName = "varchar(80)")]
    public string DescricaoCurta { get; set; }

    [Display(Name = "Descrição detalhada")]
    public string DescricaoDetalhada { get; set; }

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

		public virtual Categoria Categoria { get; set; }

    [NotMapped]
		public string OrderBy { get; set; }

		[NotMapped]
		public string CategoriaNome { get; set; }

		public Produto() {
			Reviews = new List<ProdutoReview>();
			AvaliacoesProdutosRelacionados = new List<ProdutoReview>();
		}
		public virtual ICollection<ProdutoReview> Reviews { get; set; }

		[NotMapped]
		public List<Produto> ProdutosRelacionados { get; set; }

		[NotMapped]
		public List<ProdutoReview> AvaliacoesProdutosRelacionados { get; set; }


		[NotMapped]
		public decimal PrecoComDescontoTemporario { get; set; }
	}
}
