using ErrorOr;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Domain.ShopDomain.ProductDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record CheckOrderItemDataCommand(int UserId) : IRequest<ErrorOr<Success>>;
    public class CheckOrderItemDataCommandHandler : IRequestHandler<CheckOrderItemDataCommand, ErrorOr<Success>>
    {
        
        private readonly IOrderRepository _orderRepository;
        private readonly IProductSellRepository _productSellRepository;
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public CheckOrderItemDataCommandHandler(IOrderRepository orderRepository, IProductSellRepository productSellRepository, IOrderDiscountRepository orderDiscountRepository)
        {
            _orderRepository = orderRepository;
            _productSellRepository = productSellRepository;
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CheckOrderItemDataCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;

            var order = await _orderRepository.GetUnpaidOrderWithItemsAsync(userId, cancellationToken);
              

            if (order is null)
            {
                return Result.Success; // هیچ سفارشی برای بررسی وجود ندارد
            }

            foreach (var seller in order.OrderSellers)
            {
                foreach (var item in seller.OrderItems)
                {
                    var productSell = await _productSellRepository.GetProductSellWithProductByIdAsync(item.ProductSellId,cancellationToken);
                    int price = item.Price;
                    int priceAfterOff = item.PriceAfterOff;
                    string unit = productSell.Unit;
                    int count = item.Count;

                    // بررسی موجودی کالا
                    if (count > productSell.Amount)
                    {
                        count = productSell.Amount;
                    }

                    // بررسی تخفیف فعال
                    var activeDiscount = await _orderDiscountRepository.GetActiveDiscountByProductIdAsync(productSell.ProductId,  cancellationToken);

                    if (activeDiscount is not null)
                    {
                        price = productSell.Price;
                        priceAfterOff = price - (activeDiscount.Percent * price / 100);
                    }

                    // بروزرسانی آیتم سفارش
                    item.Edit(count, price, priceAfterOff, unit);
                }
            }

            await _orderDiscountRepository.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }

}
