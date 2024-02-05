using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {
	public class ProdutosRelacionados {

		[Key]
		public int PRId { get; set; }
		public int ProdutoPrincipalId { get; set; }
		public int ProdutosRelacionadosIds { get; set; }

		[NotMapped]
		public virtual ICollection<ProdutoReview> Reviews { get; set; }

		[NotMapped]
		public string CategoriaNome { get; set; }

		// Relacionamento com a tabela de produtos
		public Produto ProdutoPrincipal { get; set; }
		public ICollection<Produto> ProdRelacionados { get; set; }
	}
}
