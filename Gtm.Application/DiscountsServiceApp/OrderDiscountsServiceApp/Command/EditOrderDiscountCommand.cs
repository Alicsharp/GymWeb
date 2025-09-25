using ErrorOr;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using MediatR;
using Utility.Appliation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command
{
   
    public record EditOrderDiscountCommand(EditOrderDiscount Command) : IRequest<ErrorOr<Success>>;

    public class EditOrderDiscountCommandHandler: IRequestHandler<EditOrderDiscountCommand, ErrorOr<Success>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public EditOrderDiscountCommandHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }

        public async Task<ErrorOr<Success>> Handle(EditOrderDiscountCommand request, CancellationToken cancellationToken)
        {
            var command = request.Command;

            var orderDiscount = await _orderDiscountRepository.GetByIdAsync(command.Id);

            if (orderDiscount is null)
            {
                return Error.NotFound(
                    code: nameof(EditOrderDiscountCommand),
                    description: "تخفیف مورد نظر یافت نشد."
                );
            }

            DateTime startDate = command.StartDate.ToEnglishDateTime();
            DateTime endDate = command.EndDate.ToEnglishDateTime();

            if (endDate.Date < DateTime.Now.Date || endDate.Date < startDate.Date)
            {
                return Error.Validation(
                    code: nameof(EditOrderDiscountCommand),
                    description: "تاریخ پایان باید حداقل امروز باشد."
                );
            }

            if (command.Percent < 1 || command.Percent > 99)
            {
                return Error.Validation(
                    code: nameof(EditOrderDiscountCommand),
                    description: "درصد تخفیف باید بین 1 تا 99 باشد."
                );
            }

            if (command.Count < 1)
            {
                return Error.Validation(
                    code: nameof(EditOrderDiscountCommand),
                    description: "تعداد تخفیف باید بیشتر از 0 باشد."
                );
            }

            var isCodeExist = await _orderDiscountRepository.ExistsAsync(
                d => d.Code == command.Code.Trim() && d.Id != command.Id);

            if (isCodeExist)
            {
                return Error.Validation(
                    code: nameof(EditOrderDiscountCommand),
                    description: "کد تخفیف تکراری است."
                );
            }

            orderDiscount.Edit(command.Percent, command.Title, command.Code, command.Count, startDate, endDate);

            var saved = await _orderDiscountRepository.SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                return Error.Failure(
                    code: nameof(EditOrderDiscountCommand),
                    description: ValidationMessages.SystemErrorMessage
                );
            }

            return Result.Success;
        }
    }

}
