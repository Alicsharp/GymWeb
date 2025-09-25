using ErrorOr;
using Gtm.Contract.PostContract.CityContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record GetCityForEditQuery(int Id) : IRequest<ErrorOr<EditCityModel>>;
    public class GetCityForEditQueryHandler : IRequestHandler<GetCityForEditQuery, ErrorOr<EditCityModel>>
    {
        private readonly ICityRepo _cityRepository;
        private readonly ICityValidation _cityValidation;

        public GetCityForEditQueryHandler( ICityRepo cityRepository,ICityValidation cityValidation)
        {
            _cityRepository = cityRepository;
            _cityValidation = cityValidation;
        }

        public async Task<ErrorOr<EditCityModel>> Handle(GetCityForEditQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _cityValidation.ValidateCityForEdit(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت اطلاعات شهر برای ویرایش
                var city = await _cityRepository.GetCityForEditAsync(request.Id);

                // روش صحیح بررسی null و برگرداندن خطا
                if (city == null)
                {
                    return Error.NotFound(
                        code: "City.NotFound",
                        description: "شهر مورد نظر یافت نشد");
                }

                return city;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "City.FetchError",
                    description: $"خطا در دریافت اطلاعات شهر: {ex.Message}");
            }
        }
    }
}
