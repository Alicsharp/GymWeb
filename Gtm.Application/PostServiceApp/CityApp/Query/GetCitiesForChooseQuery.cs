using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record GetCitiesForChooseQuery(int stateId) : IRequest<ErrorOr<List<CityForChooseQueryModel>>>;
    public class GetCitiesForChooseQueryHandler : IRequestHandler<GetCitiesForChooseQuery, ErrorOr<List<CityForChooseQueryModel>>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _cityValidation;

        public GetCitiesForChooseQueryHandler(ICityRepo cityRepository,ICityValidation cityValidation)
        {
            _cityRepo = cityRepository;
            _cityValidation = cityValidation;
        }

        public async Task<ErrorOr<List<CityForChooseQueryModel>>> Handle(GetCitiesForChooseQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _cityValidation.ValidateStateForCitySelection(request.stateId);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                var cities = await _cityRepo
                    .GetAllQueryable()
                    .Where(b => b.StateId == request.stateId)
                    .Select(c => new CityForChooseQueryModel
                    {
                        CityCode = c.Id,
                        Title = c.Title
                    })
                    .ToListAsync(cancellationToken);

                return cities ?? new List<CityForChooseQueryModel>();
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "City.SelectionError",
                    description: $"خطا در دریافت لیست شهرها: {ex.Message}");
            }
        }
    }
}
