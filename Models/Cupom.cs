using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MasterBurger.Models {

	[Table("Cupons")]
	public class Cupom {
		[Key]
		public int CupomId { get; set; }

		public string Codigo { get; set; }
		public int DescontoPercentual { get; set; }
		public bool Status { get; set; }
		public DateTime DataInicio { get; set; }
		public DateTime DataFim { get; set; }
	}
}
