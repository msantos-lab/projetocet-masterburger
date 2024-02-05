using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Models {
  public class Encomenda {

    public string EncomendaId { get; set; }

    public string UserId { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total do Encomenda")]
    public decimal EncomendaTotal { get; set; }

    [ScaffoldColumn(false)]
    [Display(Name = "Itens no Encomenda")]
    public int TotalItensEncomenda { get; set; }

    [Display(Name = "Data do Encomenda")]
    [DataType(DataType.Text)]
    [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime EncomendaRealizada { get; set; }

    [Display(Name = "Data da conclusão da encomenda")]
    [DataType(DataType.Text)]
    [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? EncomendaConcluida { get; set; }

    public List<EncomendaDetalhe>? EncomendaItens { get; set; }

		public string Status { get; set; }

		public string CodigoCupom { get; set; }
		public int DescontoPercentual { get; set; }
	}
}
