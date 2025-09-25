using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetStateTitleQuery(int Id) : IRequest<ErrorOr<string>>;
    public class GetStateTitleQueryHandler : IRequestHandler<GetStateTitleQuery, ErrorOr<string>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public GetStateTitleQueryHandler(IStateRepo stateRepository,IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<string>> Handle(GetStateTitleQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی شناسه
                var idValidation = await _validation.ValidateStateIdAsync(request.Id);
                if (idValidation.IsError)
                    return idValidation.Errors;

                // دریافت موجودیت
                var state = await _stateRepository.GetByIdAsync(request.Id);

                // اعتبارسنجی موجودیت و عنوان
                var titleValidation = await _validation.ValidateStateTitleAsync(state.Title);
                if (titleValidation.IsError)
                    return titleValidation.Errors;

                return state.Title;
            }
 
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "State.DatabaseError",
                    description: $"خطای غیرمنتظره در دریافت عنوان استان: {ex.Message}");
            }
        }
    }
}
