using MasterBurger.Models;

namespace MasterBurger.Areas.Admin.Views.ViewsModels {
  public class AdminEcomendaDetalhesViewModel {
    public string EncomendaId { get; set; }
    public decimal EncomendaTotal { get; set; }
    public int TotalItensEncomenda { get; set; }
    public DateTime? EncomendaRealizada { get; set; }
    public DateTime? EncomendaConcluida { get; set; }
    public string Nome { get; set; }
    public string Apelido { get; set; }
    public string Email { get; set; }
    public string Telemovel { get; set; }
    public string NIF { get; set; }
    public string Morada { get; set; }
    public string Localidade { get; set; }
    public string CodigoPostal { get; set; }
    public string UserId { get; set; }
		public string Status { get; set; }
		public List<AdminProdutosEncomendaViewModel> Produtos { get; set; }

		public string CodigoCupom { get; set; }
		public int DescontoPercentual { get; set; }
	}
}
