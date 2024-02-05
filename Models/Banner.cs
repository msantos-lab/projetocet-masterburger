using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterBurger.Models {

  [Table("Banners")]
  public class Banner {

    [Key]
    public int BannerId { get; set; }

    [Display(Name = "Titulo Principal")]
    [Column(TypeName = "varchar(max)")]
    public string TituloPrincipal { get; set; }

		[Display(Name = "Link")]
    [Column(TypeName = "varchar(max)")]
    public string Link { get; set; }

		[Display(Name = "Comportamento")]
		[Column(TypeName = "varchar(max)")]
		public string Comportamento { get; set; }


		[Display(Name = "Banner")]
    [Column(TypeName = "varbinary(max)")]
    public byte[] BannerBase64 { get; set; }

    [NotMapped]
    public IFormFile ImagemBanner { get; set; }

		public string Posicao { get; set; }

		[Display(Name = "Conteúdo")]
    [Column(TypeName = "varchar(max)")]
    public string Conteudo { get; set; }

  }
}
