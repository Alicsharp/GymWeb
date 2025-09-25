using ErrorOr;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query
{
    public record GetForEditOrderDiscountQuery(int id) : IRequest<ErrorOr<EditOrderDiscount>>;

    public class GetForEditOrderDiscountQueryHandler: IRequestHandler<GetForEditOrderDiscountQuery, ErrorOr<EditOrderDiscount>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public GetForEditOrderDiscountQueryHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<EditOrderDiscount>> Handle(GetForEditOrderDiscountQuery request, CancellationToken cancellationToken)
        {
            var discount = await _orderDiscountRepository.GetByIdAsync(request.id);

            if (discount is null)
            {
                return Error.NotFound(
                    code: nameof(GetForEditOrderDiscountQuery),
                    description: "تخفیف مورد نظر یافت نشد."
                );
            }

            if (discount.Type == OrderDiscountType.OrderSeller)
            {
                return Error.Validation(
                    code: nameof(GetForEditOrderDiscountQuery),
                    description: "تخفیف فروشنده قابل ویرایش نیست."
                );
            }

            return new EditOrderDiscount
            {
                Id = discount.Id,
                Title = discount.Title,
                Code = discount.Code,
                Count = discount.Count,
                Percent = discount.Percent,
                StartDate = discount.StartDate.ToPersainDatePicker(),
                EndDate = discount.EndDate.ToPersainDatePicker()
            };
        }
    }

}
