using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp
{
    public  interface IOrderDiscountValidator
    {
        Task<ErrorOr<Success>> ValidateCanApplyAsync(int discountId, CancellationToken cancellationToken = default);
    
    }
    public class OrderDiscountValidator : IOrderDiscountValidator
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public OrderDiscountValidator(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCanApplyAsync(int discountId, CancellationToken cancellationToken = default)
        {
            // 1. بررسی وجود تخفیف 
            var discount = await _orderDiscountRepository.GetByIdAsync(discountId, cancellationToken);

            if (discount is null)
            {
                return Error.NotFound("Discount.NotFound", "کد تخفیف مورد نظر یافت نشد.");
            }

            // 2. بررسی ظرفیت استفاده
            // اگر تعداد استفاده شده (Use) بیشتر یا مساوی تعداد کل مجاز (Count) باشد
            if (discount.Use >= discount.Count)
            {
                return Error.Validation("Discount.Finished", "ظرفیت استفاده از این کد تخفیف تکمیل شده است.");
            }

            // 3. بررسی تاریخ (منقضی شدن)
            if (discount.EndDate < DateTime.Now)
            {
                return Error.Validation("Discount.Expired", "مهلت استفاده از این کد تخفیف به پایان رسیده است.");
            }

            // 4. بررسی تاریخ (هنوز شروع نشده)
            if (discount.StartDate > DateTime.Now)
            {
                return Error.Validation("Discount.NotYetStarted", "زمان استفاده از این کد تخفیف هنوز فرا نرسیده است.");
            }

            return Result.Success;
        }

       
    }
}
