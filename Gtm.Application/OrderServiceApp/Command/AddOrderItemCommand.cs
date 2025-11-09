using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Domain.ShopDomain.OrderDomain.OrderItemDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record AddOrderItemCommand(int UserId, int ProductSellId) : IRequest<ErrorOr<Success>>;
    public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly IOrderRepository _orderRepository;

        public AddOrderItemCommandHandler(
            IProductSellRepository productSellRepository,
            IOrderRepository orderRepository)
        {
            _productSellRepository = productSellRepository;
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
        {
            // 1. واکشی محصول
            var productSell = await _productSellRepository.GetByIdAsync(request.ProductSellId);

            // 2. بررسی نال بودن محصول (رفع باگ کد اول)
            if (productSell == null)
            {
                return Error.NotFound(description: $"محصولی با شناسه {request.ProductSellId} یافت نشد.");
            }

            // 3. بررسی موجودی (از کد اول)
            if (productSell.Amount < 1)
            {
                return Error.Validation(description: "موجودی نداریم .");
            }

            // 4. واکشی سبد خرید باز کاربر
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.UserId);
            if (order.OrderSellers.Any(o => o.SellerId == productSell.SellerId) == false)
            {
                OrderSeller orderSeller = new(productSell.SellerId);
                OrderItem orderItem = new(productSell.Id, 1, productSell.Price, productSell.Price, productSell.Unit);
                orderSeller.AddOrderItem(orderItem);
                order.AddOrderSeller(orderSeller);
            }
            else
            {
                OrderSeller orderSeller = order.OrderSellers.Single(o => o.SellerId == productSell.SellerId);
                if (orderSeller.OrderItems.Any(i => i.ProductSellId == productSell.Id) == false)
                {
                    OrderItem orderItem = new(productSell.Id, 1, productSell.Price, productSell.Price, productSell.Unit);
                    orderSeller.AddOrderItem(orderItem);
                }
                else
                {
                    OrderItem orderItem = orderSeller.OrderItems.Single(i => i.ProductSellId == productSell.Id);
                    if (productSell.Amount < (orderItem.Count + 1)) return Error.Validation(description: "موجودی نداریم .");
                    orderItem.PlusCount(1);
                }
                orderSeller.AddPostPrice(0, 0, "");
            }
            if (await _orderRepository.SaveChangesAsync())
                return Result.Success;
               return Error.Failure(description: ValidationMessages.SystemErrorMessage);
 

        }
    }
}