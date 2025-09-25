using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record ExistTitleForCreateQuery(string Title) : IRequest<ErrorOr<Success>>;

    public class ExistTitleForCreateQueryHandler : IRequestHandler<ExistTitleForCreateQuery, ErrorOr<Success>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public ExistTitleForCreateQueryHandler(
            IStateRepo stateRepository,
            IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(
            ExistTitleForCreateQuery request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی عنوان
            var validationResult = await _validation.ValidateStateTitleAsync(request.Title);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // بررسی وجود عنوان
            var exists = await _stateRepository.ExistsAsync(s => s.Title == request.Title);

            return exists
                ? Error.Conflict(
                    code: "State.DuplicateTitle",
                    description: "استانی با این عنوان قبلاً ثبت شده است")
                : Result.Success;
        }
    }
}
