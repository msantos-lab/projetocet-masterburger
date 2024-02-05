using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {

	[Table("Categorias")]
	public class Categoria {

		[Key]
		public int CategoriaId { get; set; }

		[StringLength(100, ErrorMessage = "O tamanho máximo é de 100 caracteres")]
		[Required(ErrorMessage = "Informe o nome da categoria")]
		[Column(TypeName = "varchar(100)")]
		[Display(Name = "Nome Categoria")]
		public string CategoriaNome { get; set; }

		[Required(ErrorMessage = "Informe a descrição da categoria")]
		[Column(TypeName = "varchar(max)")]
		[Display(Name = "Descrição Categoria")]
		public string CategoriaDescricao { get; set; }

		[Required(ErrorMessage = "Informe o tipo da categoria")]
		[Column(TypeName = "varchar(100)")]
		[Display(Name = "Tipo Categoria")]
		public string CategoriaTipo { get; set; }

		public List<Produto> Produtos { get; set; }
  }
}
