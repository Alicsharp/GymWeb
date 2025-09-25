using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
    public record MinusUseOrderDiscountCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class MinusUseOrderDiscountCommandHandler : IRequestHandler<MinusUseOrderDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public MinusUseOrderDiscountCommandHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(MinusUseOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            var discountId = request.Id;

            // دریافت تخفیف با id مشخص
            var orderDiscount = await _orderDiscountRepository.GetByIdAsync(discountId);

            if (orderDiscount is null)
            {
                return Error.NotFound(
                    code: nameof(MinusUseOrderDiscountCommand),
                    description: "تخفیف مورد نظر یافت نشد."
                );
            }

            // کاهش استفاده از تخفیف
            orderDiscount.UseMinus();

            // ذخیره تغییرات
            var saved = await _orderDiscountRepository.SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                return Error.Failure(
                    code: nameof(MinusUseOrderDiscountCommand),
                    description: "کاهش استفاده از تخفیف با خطا مواجه شد."
                );
            }

            return Result.Success;
        }
    }

}
