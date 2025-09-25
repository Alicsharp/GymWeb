using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record ExistTitleForCreateQuery(string Title, int StateId) : IRequest<ErrorOr<Success>>;
    public class ExistTitleForCreateQueryHandler : IRequestHandler<ExistTitleForCreateQuery, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _cityValidatio;

        public ExistTitleForCreateQueryHandler(ICityRepo cityRepository, ICityValidation cityValidatio)
        {
            _cityRepo = cityRepository;
            _cityValidatio = cityValidatio;
        }
        public async Task<ErrorOr<Success>> Handle(ExistTitleForCreateQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _cityValidatio.ExistTitleForCreateCityValidation(request.Title, request.StateId);
            if (validationResult.IsError) return validationResult.Errors;

            var result= await _cityRepo.ExistsAsync(c => c.Title == request.Title && c.StateId == request.StateId);
            if(result == false) return Result.Success;
            return Error.Failure("ExistTtile", "این عنوان برای شهر از قبل وجود دارد");
        }
    }
}
