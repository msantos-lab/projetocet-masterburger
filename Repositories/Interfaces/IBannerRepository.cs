using MasterBurger.Models;

namespace MasterBurger.Repositories.Interfaces {
  public interface IBannerRepository {
		IEnumerable<Banner> Banners { get; }

		IEnumerable<Banner> ObterBannersPorPosicao(string posicao);
	}
}
