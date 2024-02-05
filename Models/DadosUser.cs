using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {
  public class DadosUser {
    [Key]
    public int DadosUserId { get; set; }

    [Column(TypeName = "nvarchar(450)")]
    public string UserId { get; set; }

    // Relacionamento com a tabela DadosUser
    [Column(TypeName = "varchar(450)")]
    public string DadosUtilId { get; set; }
  }
}
