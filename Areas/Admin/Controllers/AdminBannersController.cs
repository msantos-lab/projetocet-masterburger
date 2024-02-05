using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Areas.Admin.Controllers {
	[Authorize]
	[Area("Admin")]
	public class AdminBannersController : Controller {
		private readonly ApplicationDbContext _context;

		public AdminBannersController(ApplicationDbContext context) {
			_context = context;
		}

		// GET: Admin/AdminBanners
		public async Task<IActionResult> Index() {
			var banners = _context.Banners.ToList();

			return View(banners);
		}

		public IActionResult Create() {
			var produtosDisponiveis = _context.Produtos.ToList();
			ViewBag.ProdutosDisponiveis = new SelectList(produtosDisponiveis, "ProdutoId", "Nome");

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BannerViewModel viewModel, IFormFile ImagemBanner) {
			if (ImagemBanner != null) {
				viewModel.BannerBase64 = ConverterImagem(ImagemBanner);
			}

			if (ModelState.IsValid) {
				var banner = new Banner {
					TituloPrincipal = viewModel.TituloPrincipal,
					Link = viewModel.Link,
					Comportamento = viewModel.Comportamento,
					BannerBase64 = viewModel.BannerBase64,
					Posicao = viewModel.Posicao,
					Conteudo = viewModel.Conteudo,
				};

				_context.Add(banner);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			return View(viewModel);
		}


		public async Task<IActionResult> Edit(int? id) {
			if (id == null || _context.Banners == null) {
				return NotFound();
			}

			var banner = await _context.Banners.FindAsync(id);
			if (banner == null) {
				return NotFound();
			}

			return View(banner);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Banner banner, IFormFile ImagemBanner) {
			if (id != banner.BannerId) {
				return NotFound();
			}

			if (id != 0) {
				try {
					var existingBanner = _context.Banners.Find(id);

					if (existingBanner != null) {
						if (ImagemBanner != null) {
							existingBanner.BannerBase64 = ConverterImagem(ImagemBanner);
						}

						existingBanner.TituloPrincipal = banner.TituloPrincipal;
						existingBanner.Link = banner.Link;
						existingBanner.Comportamento = banner.Comportamento;
						existingBanner.Posicao = banner.Posicao;
						existingBanner.Conteudo = banner.Conteudo;

						_context.Entry(existingBanner).State = EntityState.Modified;
						await _context.SaveChangesAsync();
					} else {
						return NotFound();
					}

					return RedirectToAction(nameof(Index));
				} catch (DbUpdateConcurrencyException) {
					if (!BannerExists(banner.BannerId)) {
						return NotFound();
					} else {
						throw;
					}
				}
			}
			return View(banner);
		}
		public async Task<IActionResult> Delete(int? id) {
			if (id == null || _context.Banners == null) {
				return NotFound();
			}

			var banner = await _context.Banners
					.FirstOrDefaultAsync(m => m.BannerId == id);
			if (banner == null) {
				return NotFound();
			}

			return View(banner);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id) {
			if (_context.Banners == null) {
				return Problem("Entity set 'ApplicationDbContext.Banners'  is null.");
			}
			var banner = await _context.Banners.FindAsync(id);
			if (banner != null) {
				_context.Banners.Remove(banner);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}


		private bool BannerExists(int id) {
			return (_context.Banners?.Any(e => e.BannerId == id)).GetValueOrDefault();
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
	}
}
