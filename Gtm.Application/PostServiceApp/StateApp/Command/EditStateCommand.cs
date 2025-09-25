using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Command
{
    public record EditStateCommand(EditStateModel Command) : IRequest<ErrorOr<Success>>;

    public class EditStateCommandHandler : IRequestHandler<EditStateCommand, ErrorOr<Success>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public EditStateCommandHandler(IStateRepo stateRepository,IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(
            EditStateCommand request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validation.ValidateEditStateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // دریافت و ویرایش استان
            var state = await _stateRepository.GetByIdAsync(request.Command.Id);
            state.Edit(request.Command.Title);

            // ذخیره تغییرات
            var saveResult = await _stateRepository.SaveChangesAsync(cancellationToken);

            return saveResult
                ? Result.Success
                : Error.Failure(
                    code: "State.SaveFailed",
                    description: "خطا در ذخیره تغییرات استان");
        }
    }
}
