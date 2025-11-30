using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    public record AddOrderDiscountCommand(int userId, int id, string title, int percent) : IRequest<ErrorOr<Success>>;

    public class AddOrderDiscountCommandHandler : IRequestHandler<AddOrderDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDiscountValidator _discountValidator; 
        public AddOrderDiscountCommandHandler(IOrderRepository orderRepository, IOrderDiscountValidator discountValidator)
        {
            _orderRepository = orderRepository;
            _discountValidator = discountValidator;
        }

        public async Task<ErrorOr<Success>> Handle(AddOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            // ۱. اعتبارسنجی اولیه تخفیف (وجود، تاریخ، ظرفیت)
            var validationResult = await _discountValidator.ValidateCanApplyAsync(request.id, cancellationToken);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // ۲. دریافت سفارش باز کاربر (با توکن)
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.userId, cancellationToken);

            // ۳. گارد کلاز: اگر سفارش باز وجود نداشت
            if (order is null)
            {
                return Error.NotFound("Order.NotFound", "سفارش فعالی برای این کاربر یافت نشد.");
            }

            // ۴. اعمال تخفیف روی دامنه سفارش
            order.AddDiscount(request.id, request.percent, request.title);

            // ۵. ذخیره تغییرات
            var res = await _orderRepository.SaveChangesAsync(cancellationToken);

            // ۶. تصحیح منطق: اگر ذخیره نشد، ارور بده
            if (!res)
            {
                return Error.Failure("Database.SaveError", "اعمال تخفیف با خطا مواجه شد.");
            }

            return Result.Success;
        }
    }
}
