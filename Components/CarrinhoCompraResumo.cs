using MasterBurger.Models;
using MasterBurger.ViewsModels;
using Microsoft.AspNetCore.Mvc;

namespace MasterBurger.Components {
  public class CarrinhoCompraResumo : ViewComponent {

    private readonly CarrinhoCompra _carrinhoCompra;

    public CarrinhoCompraResumo(CarrinhoCompra carrinhoCompra) {
      _carrinhoCompra = carrinhoCompra;
    }

    public IViewComponentResult Invoke() {
      var itens = _carrinhoCompra.GetCarrinhoCompraItens();

      _carrinhoCompra.CarrinhoCompraItems = itens;

      var carrinhoCompraVM = new CarrinhoCompraViewModel {
        CarrinhoCompra = _carrinhoCompra,
        CarrinhoCompraTotal = _carrinhoCompra.GetCarrinhoCompraTotal()
      };

      return View(carrinhoCompraVM);
    }
  }
}
