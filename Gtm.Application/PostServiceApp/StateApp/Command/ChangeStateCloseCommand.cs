using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Command
{
    public record ChangeStateCloseCommand(int Id, List<int> StateCloses) : IRequest<ErrorOr<Success>>;

    public class ChangeStateCloseCommandHandler : IRequestHandler<ChangeStateCloseCommand, ErrorOr<Success>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public ChangeStateCloseCommandHandler(IStateRepo stateRepository,IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(
            ChangeStateCloseCommand request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی اولیه
            var validationResult = _validation.ValidateStateCloseRequest(request.Id, request.StateCloses);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // اعتبارسنجی وجود استان
            var existenceResult = await _validation.ValidateStateExists(request.Id);
            if (existenceResult.IsError)
            {
                return existenceResult.Errors;
            }

            // دریافت و ویرایش استان
            var state = await _stateRepository.GetByIdAsync(request.Id);
            state.ChangeCloseStates(request.StateCloses);

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
