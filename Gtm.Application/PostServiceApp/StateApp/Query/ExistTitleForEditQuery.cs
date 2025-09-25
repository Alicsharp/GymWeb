using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record ExistTitleForEditQuery(string Title, int Id) : IRequest<ErrorOr<bool>>;

    public class ExistTitleForEditQueryHandler : IRequestHandler<ExistTitleForEditQuery, ErrorOr<bool>>
    {
        private readonly IStateValidation _stateValidation;
        private readonly IStateRepo _stateRepository;

        public ExistTitleForEditQueryHandler(IStateValidation stateValidation,IStateRepo stateRepository)
        {
            _stateValidation = stateValidation;
            _stateRepository = stateRepository;
        }

        public async Task<ErrorOr<bool>> Handle(ExistTitleForEditQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی عنوان
                var validationResult = await _stateValidation.ValidateStateTitleAsync(request.Title);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // اعتبارسنجی شناسه
                if (request.Id <= 0)
                {
                    return Error.Validation("State.InvalidId", "شناسه استان باید عددی مثبت باشد.");
                }

                // بررسی وجود عنوان استان
                return await _stateRepository.ExistsAsync(s => s.Title == request.Title && s.Id != request.Id);
            }
            catch (Exception ex)
            {
                return Error.Unexpected("State.UnexpectedError", $"خطای غیرمنتظره: {ex.Message}");
            }
        }
    }
}
