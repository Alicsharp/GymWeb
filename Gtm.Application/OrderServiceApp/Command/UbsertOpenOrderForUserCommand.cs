using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.OrderContract.Command;
using Gtm.Domain.ShopDomain.OrderDomain.OrderItemDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record UbsertOpenOrderForUserCommand(int _userId, List<ShopCartViewModel> cart):IRequest<ErrorOr<Success>>;
    public class UbsertOpenOrderForUserCommandHandler : IRequestHandler<UbsertOpenOrderForUserCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductSellRepository _productSellRepository;

        public UbsertOpenOrderForUserCommandHandler(IOrderRepository orderRepository, IProductSellRepository productSellRepository)
        {
            _orderRepository = orderRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<Success>> Handle(UbsertOpenOrderForUserCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOpenOrderForUserAsync(request._userId);
            foreach (var item in request.cart)
            {
                var productSell = await _productSellRepository.GetByIdAsync(item.productSellId);

                if (order.OrderSellers.Any(o => o.SellerId == productSell.SellerId) == false)
                {
                    OrderSeller orderSeller = new(productSell.SellerId);
                    OrderItem orderItem = new(item.productSellId, item.count, item.price, item.priceAfterOff, item.unit);
                    orderSeller.AddOrderItem(orderItem);
                    order.AddOrderSeller(orderSeller);
                }
                else
                {
                    OrderSeller orderSeller = order.OrderSellers.Single(o => o.SellerId == productSell.SellerId);
                    if (orderSeller.OrderItems.Any(i => i.ProductSellId == item.productSellId) == false)
                    {
                        OrderItem orderItem = new(item.productSellId, item.count, item.price, item.priceAfterOff, item.unit);
                        orderSeller.AddOrderItem(orderItem);
                    }
                    else
                    {
                        OrderItem orderItem = orderSeller.OrderItems.Single(i => i.ProductSellId == item.productSellId);
                        orderItem.PlusCount(item.count);
                    }
                }
            }
            if(await _orderRepository.SaveChangesAsync(cancellationToken))
            {
                return Result.Success;
            }
            return Error.Failure() ;
        }
    }
}
