using ErrorOr;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query
{ 
 

    /// <summary>
    /// تعریف کوئری (یا کامند) که کد تخفیف را به عنوان ورودی می‌گیرد
    /// </summary>
    public record GetOrderDiscountForAddOrderdiscountQuery(string code) : IRequest<ErrorOr<OperationResultOrderDiscount>>;

    /// <summary>
    /// هندلر اصلی که منطق اعتبارسنجی و استفاده از کد تخفیف را پیاده‌سازی می‌کند
    /// </summary>
    public class GetOrderDiscountForAddOrderdiscountQueryHandler : IRequestHandler<GetOrderDiscountForAddOrderdiscountQuery, ErrorOr<OperationResultOrderDiscount>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public GetOrderDiscountForAddOrderdiscountQueryHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<OperationResultOrderDiscount>> Handle(GetOrderDiscountForAddOrderdiscountQuery request, CancellationToken cancellationToken)
        {
            var orderDiscount = await _orderDiscountRepository.GetByCodeAsync(request.code);

            // اعتبارسنجی 1 و 2: یافت نشدن یا متعلق به فروشگاه بودن
            if (orderDiscount == null || orderDiscount.ShopId != 0)
            {
                return Error.NotFound(description: $"تخفیفی با کد {request.code} یافت نشد .");
            }

            // اعتبارسنجی 3: تاریخ شروع
            if (orderDiscount.StartDate.Date > DateTime.Now.Date)
            {
                return Error.Validation(description: $"تاریخ شروع تخفیف {request.code} از {DateTime.Now.ToPersainDate()} است .");
            }

            // اعتبارسنجی 4: تاریخ پایان
            if (orderDiscount.EndDate.Date < DateTime.Now.Date)
            {
                return Error.Validation(description: $" تخفیف {request.code} در تاریخ {DateTime.Now.ToPersainDate()} به اتمام رسیده است .");
            }

            // اعتبارسنجی 5: تعداد استفاده
            if (orderDiscount.Use == orderDiscount.Count)
            {
                return Error.Validation(description: $"تعداد استفاده از کد تخفیف {request.code} به اتمام رسیده است .");
            }

            // --- مسیر موفقیت‌آمیز ---

            // اجرای عملیات (تغییر وضعیت)
            orderDiscount.UsePlus();
            await _orderDiscountRepository.SaveChangesAsync();

            // بازگرداندن نتیجه موفقیت‌آمیز با استفاده از کلاس OperationResultOrderDiscount
            // (دقیقا مشابه چیزی که در کد اصلی شما بود)
            return new OperationResultOrderDiscount(
                true,
                $"کد تخفیف {request.code} با موفقیت اضافه شد .",
                $"تخفیف {orderDiscount.Title} با کد {orderDiscount.Code}",
                orderDiscount.Id, // با فرض اینکه orderDiscount.Id از نوع int است
                orderDiscount.Percent
            );
        }
    }
   
}
