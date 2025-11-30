using ErrorOr;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query
{
    public record GetDiscountByCodeQuery(int ShopId, string Code): IRequest<ErrorOr<OrderDiscount>>;
    public class GetDiscountByCodeQueryHandler: IRequestHandler<GetDiscountByCodeQuery, ErrorOr<OrderDiscount>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public GetDiscountByCodeQueryHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<OrderDiscount>> Handle(GetDiscountByCodeQuery request, CancellationToken cancellationToken)
        {
            // گرفتن تخفیف بر اساس Code
            var discount = await _orderDiscountRepository.GetByCodeAsync(request.Code);

            if (discount is null || discount.ShopId != request.ShopId)
                return Error.NotFound(description: $"تخفیفی با کد {request.Code} یافت نشد.");

            if (discount.StartDate.Date > DateTime.Now.Date)
                return Error.Validation(description: $"تاریخ شروع تخفیف {request.Code} از {DateTime.Now.ToPersianDate()} است.");

            if (discount.EndDate.Date < DateTime.Now.Date)
                return Error.Validation(description: $"تخفیف {request.Code} در تاریخ {DateTime.Now.ToPersianDate()} به اتمام رسیده است.");

            if (discount.Use >= discount.Count)
                return Error.Validation(description: $"تعداد استفاده از کد تخفیف {request.Code} به اتمام رسیده است.");

            return discount;
        }
    }


}
