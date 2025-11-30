using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query
{
    public record GetOrderDiscountForAddOrderSellerdiscountQuery(int Id, string Code): IRequest<ErrorOr<Success>>;

    public class GetOrderDiscountForAddOrderSellerdiscountQueryHandler
        : IRequestHandler<GetOrderDiscountForAddOrderSellerdiscountQuery, ErrorOr<Success>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public GetOrderDiscountForAddOrderSellerdiscountQueryHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(GetOrderDiscountForAddOrderSellerdiscountQuery request, CancellationToken cancellationToken)
        {
            var orderDiscount = await _orderDiscountRepository.GetByCodeAsync(request.Code);

            if (orderDiscount is null || orderDiscount.ShopId != request.Id)
                return Error.NotFound(description: $"تخفیفی با کد {request.Code} یافت نشد.");

            if (orderDiscount.StartDate.Date > DateTime.Now.Date)
                return Error.Validation(description: $"تاریخ شروع تخفیف {request.Code} از {DateTime.Now.ToPersianDate()} است.");

            if (orderDiscount.EndDate.Date < DateTime.Now.Date)
                return Error.Validation(description: $"تخفیف {request.Code} در تاریخ {DateTime.Now.ToPersianDate()} به اتمام رسیده است.");

            if (orderDiscount.Use >= orderDiscount.Count)
                return Error.Validation(description: $"تعداد استفاده از کد تخفیف {request.Code} به اتمام رسیده است.");

            // افزایش تعداد استفاده
            orderDiscount.UsePlus();
            await _orderDiscountRepository.SaveChangesAsync(cancellationToken);

            // موفقیت
            return Result.Success;
        }
    }

}
