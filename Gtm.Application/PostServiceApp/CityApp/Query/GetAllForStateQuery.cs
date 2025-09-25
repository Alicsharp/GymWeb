using ErrorOr;
using Gtm.Contract.PostContract.CityContract.Query;
using MediatR;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record GetAllForStateQuery(int StateId) : IRequest<ErrorOr<List<CityViewModel>>>;
    public class GetAllForStateQueryHandler : IRequestHandler<GetAllForStateQuery, ErrorOr<List<CityViewModel>>>
    {
        private readonly ICityRepo  _cityRepo;
        private readonly ICityValidation _cityValidation;

        public GetAllForStateQueryHandler(ICityRepo cityRepo, ICityValidation cityValidation)
        {
            _cityRepo = cityRepo;
            _cityValidation = cityValidation;
        }

        public async Task<ErrorOr<List<CityViewModel>>> Handle(GetAllForStateQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input using validation service
                var validationResult = await _cityValidation.ValidateStateIdForGetAll(request.StateId);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // Get cities for state
                var cities = await _cityRepo.GetAllForStateAsync(request.StateId);

                // Return empty list if no cities found (not considered an error)
                return cities ?? new List<CityViewModel>();
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "City.FetchError",
                    description: $"خطا در دریافت شهرهای استان: {ex.Message}");
            }
        }
    }
}
