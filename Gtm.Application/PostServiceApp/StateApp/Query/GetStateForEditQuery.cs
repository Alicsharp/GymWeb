using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetStateForEditQuery(int Id) : IRequest<ErrorOr<EditStateModel>>;
    public class GetStateForEditQueryHandler : IRequestHandler<GetStateForEditQuery, ErrorOr<EditStateModel>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validation;

        public GetStateForEditQueryHandler(IStateRepo stateRepository,IStateValidation validation)
        {
            _stateRepository = stateRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<EditStateModel>> Handle(GetStateForEditQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی شناسه
                var idValidation = await _validation.ValidateStateIdAsync(request.Id);
                if (idValidation.IsError)
                    return idValidation.Errors;

                // دریافت داده
                var state = await _stateRepository.GetStateForEditAsync(request.Id);
                if (state == null)
                    return Error.NotFound("State.NotFound", "استان مورد نظر یافت نشد");

                // اعتبارسنجی مدل
                var modelValidation = await _validation.ValidateStateForEditAsync(state);
                if (modelValidation.IsError)
                    return modelValidation.Errors;

                return state;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "State.UnexpectedError",
                    description: $"خطای غیرمنتظره در دریافت اطلاعات ویرایش: {ex.Message}");
            }
        }
    }
}
