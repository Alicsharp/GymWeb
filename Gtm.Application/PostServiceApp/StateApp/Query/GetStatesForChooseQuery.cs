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
    public record GetStatesForChooseQuery : IRequest<ErrorOr<List<StateForChooseQueryModel>>>;

    public class GetStatesForChooseQueryHandler: IRequestHandler<GetStatesForChooseQuery, ErrorOr<List<StateForChooseQueryModel>>>
    {
        private readonly IStateRepo _stateRepository;

        public GetStatesForChooseQueryHandler(IStateRepo stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public async Task<ErrorOr<List<StateForChooseQueryModel>>> Handle(GetStatesForChooseQuery request,CancellationToken cancellationToken)
        {
            try
            {
                var states = await _stateRepository.GetStatesForChooseAsync( );

                return states switch
                {
                    null => Error.NotFound(
                        code: "States.NotFound",
                        description: "لیست استان‌ها یافت نشد"),

                    { Count: 0 } => new List<StateForChooseQueryModel>(), // لیست خالی

                    _ => states // نتیجه موفق
                };
            }
            
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "States.UnexpectedError",
                    description: $"خطای غیرمنتظره: {ex.Message}");
            }
        }
    }
}
