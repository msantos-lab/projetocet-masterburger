using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Controllers {
	public class ProdutoController : Controller {
		private readonly IProdutoRepository _produtoRepository;
		private readonly ICategoriaRepository _categoriaRepository;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<IdentityUser> _userManager;

		public ProdutoController(IProdutoRepository produtoRepository, ApplicationDbContext context, ICategoriaRepository categoriaRepository, UserManager<IdentityUser> userManager) {
			_produtoRepository = produtoRepository;
			_categoriaRepository = categoriaRepository;
			_context = context;
			_userManager = userManager;
		}


		public IActionResult List() {
			var produtos = _produtoRepository.Produtos;
			var orderByOptions = GetOrderByOptions();

			var produtosListViewModel = new ProdutoListViewModel {
				Produtos = produtos,
				OrderByOptions = orderByOptions
			};

			return View("List", produtosListViewModel);
		}

		public IActionResult ListCat(string categoria, string order) {
			IEnumerable<Produto> produtos;
			string categoriaAtual = string.Empty;
			string categoriaAtualDescricao = string.Empty;

			if (string.IsNullOrEmpty(categoria)) {
				produtos = _produtoRepository.Produtos;
				categoriaAtual = "Todos os produtos";
			} else {
				produtos = _produtoRepository.Produtos.Where(p => p.Categoria.CategoriaNome.Equals(categoria));
				categoriaAtual = categoria;
				categoriaAtualDescricao = _categoriaRepository.Categorias
					.Where(q => q.CategoriaNome.Equals(categoria))
					.Select(q => q.CategoriaDescricao)
					.FirstOrDefault();
			}

			var orderByOptions = GetOrderByOptions();

			var produtosOrdenados = produtos.ToList();

			if (order == "ordenaraz") {
				produtosOrdenados = produtosOrdenados.OrderBy(p => p.Nome).ToList();
			} else if (order == "ordenarza") {
				produtosOrdenados = produtosOrdenados.OrderByDescending(p => p.Nome).ToList();
			} else if (order == "preco-asc") {
				produtosOrdenados = produtosOrdenados.OrderBy(p => p.Preco).ToList();
			} else if (order == "preco-desc") {
				produtosOrdenados = produtosOrdenados.OrderByDescending(p => p.Preco).ToList();
			}

			var produtosListViewModel = new ProdutoListViewModel {
				Produtos = produtosOrdenados,
				CategoriaAtual = categoriaAtual,
				CategoriaAtualDescricao = categoriaAtualDescricao,
				OrderByOptions = orderByOptions,
				CurrentOrderBy = order
			};

			return View(produtosListViewModel);
		}

		[Route("produto/novidades")]
		public IActionResult Novidades() {
			var produtos = _produtoRepository.Produtos
					.OrderByDescending(p => p.ProdutoId)
					.ToList();

			return View(produtos);
		}

		private List<SelectListItem> GetOrderByOptions() {
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "Ordenar por", Value = "menu_order" },
				new SelectListItem { Text = "--", Value = "recentes" },
				new SelectListItem { Text = "A-Z", Value = "ordenaraz" },
				new SelectListItem { Text = "Z-A", Value = "ordenarza" },
				new SelectListItem { Text = "Preço (mais baixo)", Value = "preco-desc" },
				new SelectListItem { Text = "Preço (mais alto)", Value = "preco-asc" }
			};
		}


		public IActionResult Details(int produtoId) {
			var produto = _context.Produtos
					.Include(r => r.Reviews)
					.Include(c => c.Categoria)
					.FirstOrDefault(p => p.ProdutoId == produtoId);

			if (produto == null) {
				return NotFound();
			}

			var produtosRelacionadosIds = _context.ProdutosRelacionados
					.Where(pr => pr.ProdutoPrincipalId == produtoId)
					.Select(pr => pr.ProdutosRelacionadosIds)
					.ToList();

			var produtosRelacionados = _context.Produtos
					.Include(r => r.Reviews)  // Inclui avaliações dos produtos relacionados
					.Where(p => produtosRelacionadosIds.Contains(p.ProdutoId))
					.ToList();

			// Adiciona os produtos relacionados à model
			produto.ProdutosRelacionados = produtosRelacionados;

			// Obtém as avaliações dos produtos relacionados
			var avaliacoesProdutosRelacionados = produtosRelacionados
					.SelectMany(pr => pr.Reviews)
					.ToList();

			// Adiciona as avaliações dos produtos relacionados à model
			produto.AvaliacoesProdutosRelacionados = avaliacoesProdutosRelacionados;

			return View(produto);
		}


		public IActionResult ResultadoPesquisa(string searchString) {
			IEnumerable<Produto> produtos;

			if (!string.IsNullOrWhiteSpace(searchString)) {
				// Pesquisa sem distinção entre maiúsculas e minúsculas
				produtos = _produtoRepository.Produtos
						.Where(p =>
								p.Nome.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
								p.DescricaoCurta.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
						.OrderBy(p => p.ProdutoId);

			} else {
				// Se a pesquisa estiver em branco, retorna todos os produtos
				produtos = _produtoRepository.Produtos.OrderBy(p => p.ProdutoId);
			}

			var resultadoPesquisaViewModel = new ResultadoPesquisaViewModel {
				Produtos = produtos,
			};

			ViewData["termoPesquisa"] = searchString;

			return View(resultadoPesquisaViewModel);
		}


		[HttpPost]
		public async Task<IActionResult> AdicionarReview(ProdutoReview review) {
			if (User.Identity.IsAuthenticated) {
				var user = await _userManager.GetUserAsync(User);

				if (user != null) {

					string nome = _context.DadosUtilizador.Where(d => d.IdUser == user.Id).Select(d => d.Nome).FirstOrDefault();
					string apelido = _context.DadosUtilizador.Where(d => d.IdUser == user.Id).Select(d => d.Apelido).FirstOrDefault();

					review.ClienteId = user.Id;
					review.ClienteNome = nome + " " + apelido;
					review.ClienteEmail = _context.DadosUtilizador.Where(d => d.IdUser == user.Id).Select(d => d.Email).FirstOrDefault();
					review.Status = "Nova";
					review.StatusNotCliente = false;
					review.DataCriacao = DateTime.Now;
					_context.ProdutoReviews.Add(review);
					_context.SaveChanges();

					TempData["AvaliacaoMsg"] = "A sua avaliação foi enviada com sucesso. Vamos verificar e em breve receberá o nosso e-mail.";

					return RedirectToAction("Details", new { ProdutoId = review.ProdutoId });
				}
			}

			return RedirectToAction("Details", new { ProdutoId = review.ProdutoId });
		}

	}
}
