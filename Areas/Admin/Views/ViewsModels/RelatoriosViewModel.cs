using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Areas.Admin.Views.ViewsModels {
	public class RelatoriosViewModel {
		[Display(Name = "Data de Início")]
		public DateTime DataInicio { get; set; }

		[Display(Name = "Data de Término")]
		public DateTime DataFim { get; set; }

    [Display(Name = "Relatório a ser gerado")]
    public string RelatorioSelecionado { get; set; }

		public List<object> DadosRelatorio { get; set; }
	}
}
