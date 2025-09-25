using ErrorOr;
using Gtm.Contract.SiteContract.SliderContract.Command;
using MediatR;

namespace Gtm.Application.SiteServiceApp.SliderApp.Query
{
    public record GetForEditQuery(int Id) : IRequest<ErrorOr<EditSlider>>;

    public sealed class GetForEditQueryHandler : IRequestHandler<GetForEditQuery, ErrorOr<EditSlider>>
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly ISliderValidator _validator;

        public GetForEditQueryHandler(
            ISliderRepository sliderRepository,
            ISliderValidator validator)
        {
            _sliderRepository = sliderRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EditSlider>> Handle(GetForEditQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _validator.ValidateGetForEditAsync(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت اطلاعات اسلایدر
                var slider = await _sliderRepository.GetForEditAsync(request.Id);
                if (slider == null)
                {
                    return Error.NotFound(
                    code: "Slider.NotFound",
                    description: "اسلایدر مورد نظر یافت نشد");
                }
                return slider;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Slider.FetchError",
                    description: $"خطا در دریافت اطلاعات اسلایدر: {ex.Message}");
            }
        }
    }
}
