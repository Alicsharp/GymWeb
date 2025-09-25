using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Contract.SellerContract.Command;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Command
{
    public record AddOrderSellerDiscountCommand(int UserId,int SellerId,int DiscountId,string Title,int Percent) : IRequest<ErrorOr<Success>>;

    public class AddOrderSellerDiscountCommandHandler: IRequestHandler<AddOrderSellerDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public AddOrderSellerDiscountCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(AddOrderSellerDiscountCommand request, CancellationToken cancellationToken)
        {
            // گرفتن سفارش باز برای کاربر
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.UserId);
            if (order is null)
                return Error.NotFound(description: "Open order not found for this user.");

            // بررسی اینکه این Seller در سفارش وجود دارد یا نه
            var orderSeller = order.OrderSellers.SingleOrDefault(s => s.SellerId == request.SellerId);
            if (orderSeller is null)
                return Error.NotFound(description: "Seller not found in this order.");

            // اضافه کردن تخفیف
            orderSeller.AddDiscount(request.DiscountId, request.Percent, request.Title);

            // ذخیره تغییرات
            var saveResult = await _orderRepository.SaveChangesAsync(cancellationToken);
            if (saveResult==false)
                return Error.Failure();

            return Result.Success;
        }
    }


}
