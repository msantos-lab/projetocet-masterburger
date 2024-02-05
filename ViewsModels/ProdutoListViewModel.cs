using MasterBurger.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MasterBurger.ViewsModels {
	public class ProdutoListViewModel {
		public IEnumerable<Produto> Produtos { get; set; }
		public string CategoriaAtual { get; set; }
		public string CategoriaAtualDescricao { get; set; }
		public List<SelectListItem> OrderByOptions { get; set; } 
		public string CurrentOrderBy { get; set; } 
	}
}
