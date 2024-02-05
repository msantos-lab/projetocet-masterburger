using MasterBurger.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MasterBurger.Models {
  public class CarrinhoCompra {
		private readonly ApplicationDbContext _context;

		public CarrinhoCompra(ApplicationDbContext context) {
			_context = context;
		}

		public string CarrinhoCompraId { get; set; }
		public List<CarrinhoCompraItem> CarrinhoCompraItems { get; set; }

		public static CarrinhoCompra GetCarrinho(IServiceProvider services) {

			ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

			string carrinhoId = session.GetString("CarrinhoId");

			var context = services.GetRequiredService<ApplicationDbContext>();

			if (!string.IsNullOrEmpty(carrinhoId)) {
				return new CarrinhoCompra(context) {
					CarrinhoCompraId = carrinhoId
				};
			}

			// Se não existe um carrinho na sessão, cria um novo
			carrinhoId = Guid.NewGuid().ToString();
			session.SetString("CarrinhoId", carrinhoId);

			return new CarrinhoCompra(context) {
				CarrinhoCompraId = carrinhoId
			};
		}

		public int DetalheAdicionarAoCarrinho(Produto produto, ClaimsPrincipal user, int quantidade) {
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
					s => s.Produto.ProdutoId == produto.ProdutoId &&
							 s.CarrinhoCompraId == CarrinhoCompraId);

			if (carrinhoCompraItem == null) {
				carrinhoCompraItem = new CarrinhoCompraItem {
					CarrinhoCompraId = CarrinhoCompraId,
					Produto = produto,
					Quantidade = quantidade,
				};

				_context.CarrinhoCompraItens.Add(carrinhoCompraItem);
			} else {
				carrinhoCompraItem.Quantidade += quantidade;
			}
			_context.SaveChanges();

			// Retorna a nova quantidade de itens no carrinho após a adição
			return _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Sum(c => c.Quantidade);
		}


		public int AdicionarAoCarrinho(Produto produto, ClaimsPrincipal user) {
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
					s => s.Produto.ProdutoId == produto.ProdutoId &&
							 s.CarrinhoCompraId == CarrinhoCompraId);

			if (carrinhoCompraItem == null) {
				carrinhoCompraItem = new CarrinhoCompraItem {
					CarrinhoCompraId = CarrinhoCompraId,
					Produto = produto,
					Quantidade = 1,
				};

				_context.CarrinhoCompraItens.Add(carrinhoCompraItem);
			} else {
				carrinhoCompraItem.Quantidade++;
			}
			_context.SaveChanges();

			// Retorna a nova quantidade de itens no carrinho após a adição
			return _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Sum(c => c.Quantidade);
		}


		public int AumentarQuantidade(Produto produto, int quantidadeDesejada) {
			var carrinhoCompraItem = _context.CarrinhoCompraItens
					.SingleOrDefault(s => s.Produto.ProdutoId == produto.ProdutoId && s.CarrinhoCompraId == CarrinhoCompraId);

			if (carrinhoCompraItem != null) {
				// Verifica se a soma da quantidade desejada com a quantidade no carrinho ultrapassa o disponível
				if (VerificarStockDisponivel(produto, quantidadeDesejada)) {
					if (carrinhoCompraItem.Quantidade > 0) {
						carrinhoCompraItem.Quantidade += quantidadeDesejada;
					} else {
						_context.CarrinhoCompraItens.Update(carrinhoCompraItem);
					}
					_context.SaveChanges();
				} else {
					return -1;
				}
			}

			// Retorna a nova quantidade de itens no carrinho após o aumento
			return _context.CarrinhoCompraItens
					.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Sum(c => c.Quantidade);
		}


		public bool VerificarStockDisponivel(Produto produto, int quantidadeDesejada) {
			var itemNoCarrinho = _context.CarrinhoCompraItens
					.FirstOrDefault(item => item.Produto.ProdutoId == produto.ProdutoId);

			if (itemNoCarrinho != null) {
				// Verifica se a soma da quantidade desejada com a quantidade no carrinho ultrapassa o stock disponível
				return (itemNoCarrinho.Quantidade + quantidadeDesejada) <= produto.Quantidade;
			}

			// Se o item não estiver no carrinho, verificar se a quantidade é menor ou igual ao stock total
			return quantidadeDesejada <= produto.Quantidade;
		}


		public int DiminuirQuantidade(Produto produto) {
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
					s => s.Produto.ProdutoId == produto.ProdutoId &&
							 s.CarrinhoCompraId == CarrinhoCompraId);

			if (carrinhoCompraItem != null) {
				if (carrinhoCompraItem.Quantidade > 0) {
					carrinhoCompraItem.Quantidade--;

				} else {
					_context.CarrinhoCompraItens.Update(carrinhoCompraItem);
				}
				_context.SaveChanges();
			}

			// Retorna a nova quantidade de itens no carrinho após a remoção
			return _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Sum(c => c.Quantidade);
		}


		public int RemoverDoCarrinho(Produto produto) {
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
					s => s.Produto.ProdutoId == produto.ProdutoId &&
							 s.CarrinhoCompraId == CarrinhoCompraId);

			var quantidadeLocal = 0;

			if (carrinhoCompraItem != null) {
				if (carrinhoCompraItem.Quantidade > 1) {
					carrinhoCompraItem.Quantidade--;
					quantidadeLocal = carrinhoCompraItem.Quantidade;
				} else {
					_context.CarrinhoCompraItens.Remove(carrinhoCompraItem);
				}
				_context.SaveChanges();
			}

			// Retorna a nova quantidade de itens no carrinho após a remoção
			return _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Sum(c => c.Quantidade);
		}

    public int ExcluirDoCarrinho(Produto produto) {
      var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
           s => s.Produto.ProdutoId == produto.ProdutoId &&
                s.CarrinhoCompraId == CarrinhoCompraId);

      if (carrinhoCompraItem != null) {
        _context.CarrinhoCompraItens.Remove(carrinhoCompraItem);
        _context.SaveChanges();
      }

      // Retorna a nova quantidade de itens no carrinho após a remoção
      return _context.CarrinhoCompraItens
          .Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
          .Sum(c => c.Quantidade);
    }


    public List<CarrinhoCompraItem> GetCarrinhoCompraItens() {
			return CarrinhoCompraItems ??
						 (CarrinhoCompraItems =
								 _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
										 .Include(s => s.Produto)
										 .ToList());
		}


		public void LimparCarrinho() {
			var carrinhoItens = _context.CarrinhoCompraItens
													 .Where(carrinho => carrinho.CarrinhoCompraId == CarrinhoCompraId);

			_context.CarrinhoCompraItens.RemoveRange(carrinhoItens);
			_context.SaveChanges();
		}


		public decimal GetCarrinhoCompraTotal() {
			var total = _context.CarrinhoCompraItens
					.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
					.Select(c => c.Produto.Preco * c.Quantidade)
					.Sum();

			return total;
		}


		public string CodigoCupomTemporario { get; set; }
		public decimal DescontoPercentualTemporario { get; set; }

		public void AplicarDesconto(string codigoCupom) {
			if (ValidarCupom(codigoCupom)) {
				CodigoCupomTemporario = codigoCupom;
				DescontoPercentualTemporario = ObterDescontoPercentual(codigoCupom);

				// Verifica se o cupom expirou
				var cupom = _context.Cupons.SingleOrDefault(c => c.Codigo == codigoCupom);
				if (cupom != null && cupom.DataFim < DateTime.Now) {
					cupom.Status = false;
					_context.SaveChanges(); 
				}
			}
		}

		public bool ValidarCupom(string codigoCupom) {
			var cupom = _context.Cupons.SingleOrDefault(c => c.Codigo == codigoCupom);
			return cupom != null && cupom.Status && cupom.DataInicio <= DateTime.Now && cupom.DataFim >= DateTime.Now;
		}

		private decimal ObterDescontoPercentual(string codigoCupom) {
			var cupom = _context.Cupons.SingleOrDefault(c => c.Codigo == codigoCupom);
			return cupom?.DescontoPercentual ?? 0;
		}

	}
}
