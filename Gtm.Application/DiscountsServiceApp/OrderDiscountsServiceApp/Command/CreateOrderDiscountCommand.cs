using ErrorOr;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    // رکورد ورودی
    public record CreateOrderDiscountCommand(CreateOrderDiscount Dto, OrderDiscountType Type, int ShopId) : IRequest<ErrorOr<Success>>;

    public class CreateOrderDiscountCommandHandler : IRequestHandler<CreateOrderDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public CreateOrderDiscountCommandHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            // 1. تبدیل تاریخ‌ها
            DateTime startDate;
            DateTime endDate;

            try
            {
                startDate = request.Dto.StartDate.ToEnglishDateTime();
                endDate = request.Dto.EndDate.ToEnglishDateTime();
            }
            catch
            {
                return Error.Validation("Date.Format", "فرمت تاریخ ارسالی صحیح نیست.");
            }

            // 2. اعتبارسنجی تاریخ
            if (endDate.Date < DateTime.Now.Date)
            {
                return Error.Validation("Date.Invalid", "تاریخ پایان باید حداقل امروز باشد.");
            }

            if (endDate.Date < startDate.Date)
            {
                return Error.Validation("Date.Invalid", "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
            }

            // 3. اعتبارسنجی درصد
            if (request.Dto.Percent < 1 || request.Dto.Percent > 99)
            {
                return Error.Validation("Percent.Invalid", "درصد تخفیف باید بین ۱ تا ۹۹ باشد.");
            }

            // 4. اعتبارسنجی تعداد
            if (request.Dto.Count < 1)
            {
                return Error.Validation("Count.Invalid", "تعداد تخفیف باید بیشتر از ۰ باشد.");
            }

            // 5. بررسی تکراری بودن کد تخفیف
            var isDuplicate = await _orderDiscountRepository.ExistsAsync(d => d.Code == request.Dto.Code.Trim(), cancellationToken);
            if (isDuplicate)
            {
                return Error.Conflict("Discount.Duplicate", "کد تخفیف تکراری است.");
            }

            // 6. ساخت موجودیت
            var discount = new OrderDiscount(
                percent: request.Dto.Percent,
                title: request.Dto.Title,
                code: request.Dto.Code.Trim(),
                count: request.Dto.Count,
                type: request.Type,
                startDate: startDate,
                endDate: endDate,
                shopId: request.ShopId
            );

            // 7. ذخیره در دیتابیس
            await _orderDiscountRepository.AddAsync(discount);
            var isSaved = await _orderDiscountRepository.SaveChangesAsync(cancellationToken);

            if (!isSaved)
            {
                return Error.Failure("Database.SaveError", "ذخیره‌سازی تخفیف با خطا مواجه شد.");
            }

            return Result.Success;
        }
    }
}
