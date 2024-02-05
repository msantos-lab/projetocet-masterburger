using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Models {
  public class DadosUtilizador {

    [Key]
    [Column(TypeName = "varchar(450)")]
    public string DadosUtilizadorId { get; set; }

		public string IdUser { get; set; }

		[Column(TypeName = "varchar(200)")]
    public string Nome { get; set; }

    public string Email { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string Apelido { get; set; }

    [MaxLength(9)]
    [Column(TypeName = "varchar(9)")]
    public string NIF { get; set; }

    [Column(TypeName = "varchar(max)")]
    public string Morada { get; set; }

    [Column(TypeName = "varchar(max)")]
    public string Localidade { get; set; }

    [Column(TypeName = "varchar(10)")]
    public string CodigoPostal { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string Telemovel { get; set; }

    public static implicit operator DadosUtilizador(string v) {
      throw new NotImplementedException();
    }

  }
}
