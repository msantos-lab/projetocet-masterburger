using MasterBurger.Models;

namespace MasterBurger.ViewsModels {
  public class BannerViewModel {
    public int BannerId { get; set; }
    public string TituloPrincipal { get; set; }
    public string Link { get; set; }
		public string Comportamento { get; set; }
		public IFormFile ImagemBanner { get; set; }
    public byte[] BannerBase64 { get; set; }
		public string Posicao { get; set; }
		public string Conteudo { get; set; }
	}
}
