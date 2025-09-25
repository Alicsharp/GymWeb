using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Command;
using Gtm.Domain.PostDomain.StateAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Command
{
    public record CreateStateCommand(CreateStateModel command) : IRequest<ErrorOr<Success>>;
    public class CreateStateCommandHandler : IRequestHandler<CreateStateCommand, ErrorOr<Success>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public CreateStateCommandHandler(IStateRepo stateRepository, IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(CreateStateCommand request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی ناهمزمان
            var validationResult = await _validation.ValidateCreateStateAsync(request.command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // ایجاد استان جدید
            var state = new State(request.command.Title);

            // ذخیره در دیتابیس
          await _stateRepository.AddAsync(state);
            var createResult =await _stateRepository.SaveChangesAsync(cancellationToken);

            return createResult
                ? Result.Success
                : Error.Failure(
                    code: "State.CreateFailed",
                    description: "خطا در ایجاد استان جدید");
        }
    }
}
