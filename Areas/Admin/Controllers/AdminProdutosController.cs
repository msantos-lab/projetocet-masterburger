using MasterBurger.Areas.Admin.Views.ViewsModels;
using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize]
	[Area("Admin")]
	public class AdminProdutosController : Controller {
		private readonly ApplicationDbContext _context;

		public AdminProdutosController(ApplicationDbContext context) {
			_context = context;
		}

		public async Task<IActionResult> Index(string filter, int pageindex = 1, string sort = "Nome") {
			var resultado = _context.Produtos.Include(l => l.Categoria).AsQueryable();

			if (!string.IsNullOrWhiteSpace(filter)) {
				resultado = resultado.Where(p => p.Nome.Contains(filter));
			}

			var model = await PagingList.CreateAsync(resultado, 5, pageindex, sort, "Nome");
			model.RouteValue = new RouteValueDictionary { { "filter", filter } };
			return View(model);
		}


		public async Task<IActionResult> Details(int? id) {
			if (id == null || _context.Produtos == null) {
				return NotFound();
			}

			var produto = await _context.Produtos
					.Include(p => p.Categoria)
					.FirstOrDefaultAsync(m => m.ProdutoId == id);
			if (produto == null) {
				return NotFound();
			}

			return View(produto);
		}

		public IActionResult Create() {
			ViewData["ProdutosRelacionados"] = new SelectList(_context.Produtos.ToList(), "ProdutoId", "Nome");
			ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "CategoriaNome");

			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProdutoViewModel viewModel, IFormFile ImagemProduto, int[] produtosRelacionados) {
			if (ImagemProduto != null) {
				viewModel.ImagemBase64 = ConverterImagem(ImagemProduto);
			}

			if (ModelState.IsValid) {
				try {
					// Obtem o último ID usado na tabela de Produtos
					int ultimoIdProduto = _context.Produtos.Max(p => (int?)p.ProdutoId) ?? 0;
					int produtoPrincipalId = ultimoIdProduto + 1;

					// Atualiza a tabela ProdutosRelacionados
					AtualizarProdutosRelacionados(produtoPrincipalId, produtosRelacionados);

					var produto = new Produto {
						Nome = viewModel.Nome,
						DescricaoCurta = viewModel.DescricaoCurta,
						DescricaoDetalhada = viewModel.DescricaoDetalhada,
						Preco = viewModel.Preco,
						PrecoAnterior = viewModel.PrecoAnterior,
						ImagemBase64 = viewModel.ImagemBase64,
						IsDestaque = viewModel.IsDestaque,
						IsDisponivel = viewModel.IsDisponivel,
						CategoriaId = viewModel.CategoriaId,
						Quantidade = viewModel.Quantidade,
					};

					_context.Add(produto);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				} catch (DbUpdateException ex) {
					Console.WriteLine($"Erro ao salvar na base de dados: {ex.Message}");
					ModelState.AddModelError("", "Ocorreu um erro ao salvar na base de dados.");
				}
			} else {
				var modelErrors = ModelState.Values.SelectMany(v => v.Errors);
				foreach (var error in modelErrors) {
				}
			}

			ViewData["ProdutosRelacionados"] = new SelectList(_context.Produtos.ToList(), "ProdutoId", "Nome", viewModel.ProdRelacionados);

			ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "CategoriaNome", viewModel.CategoriaId);
			return View(viewModel);
		}


		public async Task<IActionResult> Edit(int? id) {
			if (id == null || _context.Produtos == null) {
				return NotFound();
			}

			var produto = await _context.Produtos.FindAsync(id);
			if (produto == null) {
				return NotFound();
			}

			ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "CategoriaNome", produto.CategoriaId);

			var produtoPrincipal = await _context.Produtos.FindAsync(id);
			var produtosRelacionados = _context.Produtos.Where(p => p.ProdutoId != id).ToList();
			ViewData["ProdutosRelacionados"] = new SelectList(produtosRelacionados, "ProdutoId", "Nome", produtoPrincipal.ProdutoId);

			return View(produto);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Produto produto, IFormFile ImagemProduto, int[] produtosRelacionados) {
			if (id != produto.ProdutoId) {
				return NotFound();
			}

			if (id != 0) {
				try {
					var existingProduto = _context.Produtos.Find(id);

					if (existingProduto != null) {
						if (ImagemProduto != null) {
							existingProduto.ImagemBase64 = ConverterImagem(ImagemProduto);
						}

						// Obtem o ID do produto principal
						int produtoPrincipalId = existingProduto.ProdutoId;

						// Atualiza a tabela ProdutosRelacionados
						AtualizarProdutosRelacionados(produtoPrincipalId, produtosRelacionados);

						existingProduto.Nome = produto.Nome;
						existingProduto.Quantidade = produto.Quantidade;
						existingProduto.DescricaoCurta = produto.DescricaoCurta;
						existingProduto.DescricaoDetalhada = produto.DescricaoDetalhada;
						existingProduto.Preco = produto.Preco;
						existingProduto.PrecoAnterior = produto.PrecoAnterior;
						existingProduto.IsDestaque = produto.IsDestaque;
						existingProduto.IsDisponivel = produto.IsDisponivel;
						existingProduto.CategoriaId = produto.CategoriaId;

						_context.Entry(existingProduto).State = EntityState.Modified;
						await _context.SaveChangesAsync();

					} else {
						return NotFound();
					}

					return RedirectToAction(nameof(Index));
				} catch (DbUpdateConcurrencyException) {
					if (!ProdutoExists(produto.ProdutoId)) {
						return NotFound();
					} else {
						throw;
					}
				}
			}

			ViewData["CategoriaId"] = new SelectList(_context.Categorias, "CategoriaId", "CategoriaNome", produto.CategoriaId);
			return View(produto);
		}


		private bool ProdutoExists(int id) {
			return (_context.Produtos?.Any(e => e.ProdutoId == id)).GetValueOrDefault();
		}

		private byte[] ConverterImagem(IFormFile ImagemProduto) {
			Stream stream = ImagemProduto.OpenReadStream();
			long tamanho = ImagemProduto.Length;

			byte[] fotoBina = new byte[tamanho];
			stream.Read(fotoBina, 0, (int)tamanho);

			using (var memoryStream = new MemoryStream()) {
				memoryStream.Write(fotoBina, 0, (int)tamanho);
				return memoryStream.ToArray();
			}
		}


		// Atualiza ProdutosRelacionados
		private void AtualizarProdutosRelacionados(int produtoPrincipalId, int[] produtosRelacionados) {

			// Limpa os registos antigos relacionados ao produto principal
			var registosAntigos = _context.ProdutosRelacionados.Where(pr => pr.ProdutoPrincipalId == produtoPrincipalId);
			_context.ProdutosRelacionados.RemoveRange(registosAntigos);

			// Adiciona os produtos relacionados
			foreach (var produtoRelacionadoId in produtosRelacionados) {
				_context.ProdutosRelacionados.Add(new ProdutosRelacionados {
					ProdutoPrincipalId = produtoPrincipalId,
					ProdutosRelacionadosIds = produtoRelacionadoId
				});
			}

			_context.SaveChanges();
		}
	}
}
