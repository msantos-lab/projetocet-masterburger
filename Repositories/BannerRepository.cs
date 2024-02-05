using MasterBurger.Data;
using MasterBurger.Models;
using MasterBurger.Repositories.Interfaces;

namespace MasterBurger.Repositories {
  public class BannerRepository : IBannerRepository {

    private readonly ApplicationDbContext _context;

    public BannerRepository(ApplicationDbContext context) {
      _context = context;
    }

    public IEnumerable<Banner> Banners => _context.Banners;

		public IEnumerable<Banner> ObterBannersPorPosicao(string posicao) {
			return _context.Banners.Where(b => b.Posicao == posicao).ToList();
		}
	}
}

