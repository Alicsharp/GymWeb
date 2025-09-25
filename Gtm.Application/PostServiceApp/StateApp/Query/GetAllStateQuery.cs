using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Query;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetAllStateQuery : IRequest<ErrorOr<List<StateViewModel>>>;

    public class GetAllStateQueryHandler : IRequestHandler<GetAllStateQuery, ErrorOr<List<StateViewModel>>>
    {
        private readonly IStateRepo _stateRepository;
        public GetAllStateQueryHandler(IStateRepo stateRepository)
        {
            _stateRepository = stateRepository;
           
        }

        public async Task<ErrorOr<List<StateViewModel>>> Handle(GetAllStateQuery request,CancellationToken cancellationToken)
        {
            try
            {
                var states = await _stateRepository.GetAllStateViewModelAsync( );

                if (states is null)
                {
                     
                    return Error.NotFound(
                        code: "State.NotFound",
                        description: "هیچ استانی یافت نشد");
                }

                if (!states.Any())
                {
                    
                    return new List<StateViewModel>(); // بازگرداندن لیست خالی به جای خطا
                }

                return states;
            }
     
            catch (Exception ex)
            {
                
                return Error.Unexpected(
                    code: "State.UnexpectedError",
                    description: $"خطای غیرمنتظره در دریافت لیست استان‌ها: {ex.Message}");
            }
        }
    }
}
