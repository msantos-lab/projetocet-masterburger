using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {

	[Table("Reviews")]
	public class ProdutoReview {
		[Key]
		public int ReviewId { get; set; }

		[Required]
		public int ProdutoId { get; set; }

		public virtual Produto Produto { get; set; }

		[Required]
		public string ClienteId { get; set; }

		public string ClienteNome { get; set; }
    
		public string ClienteEmail { get; set; }

    [Required]
		[Range(1, 5, ErrorMessage = "A avaliação deve estar entre 1 e 5.")]
		public int Avaliacao { get; set; }

		[Required]
		[MaxLength(500, ErrorMessage = "O comentário deve ter no máximo 500 caracteres.")]
		public string Comentario { get; set; }

		public DateTime DataCriacao { get; set; }

		public string Status { get; set; }

    public bool StatusNotCliente { get; set; }
  }
}
