namespace MasterBurger.Areas.Admin.Views.ViewsModels {
  public class AdminProdutosEncomendaViewModel {
    public int ProdutoId { get; set; }
    public string Nome { get; set; }
    public string Autor { get; set; }
    public decimal Preco { get; set; }
		public int Quantidade { get; set; }
	}
}
