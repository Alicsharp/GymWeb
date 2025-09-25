using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetStatesWithCityQuery : IRequest<ErrorOr<List<StateQueryModel>>>;
    public class GetStatesWithCityQueryHandler : IRequestHandler<GetStatesWithCityQuery, ErrorOr<List<StateQueryModel>>>
    {

        private readonly IStateRepo _stateRepository;

        public GetStatesWithCityQueryHandler(IStateRepo stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public async Task<ErrorOr<List<StateQueryModel>>> Handle(GetStatesWithCityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stateRepository.GetStatesWithCityAsync();

                if (result == null || !result.Any())
                {
                    return Error.NotFound(
                        code: "States.NotFound",
                        description: "No states with cities found");
                }

                return result;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Database.Error",
                    description: $"Error retrieving states: {ex.Message}");
            }
        }
    }
}
