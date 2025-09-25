using ErrorOr;
using Gtm.Application.SiteServiceApp.SliderApp;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SiteServiceApp.SliderApp.Command
{
    public record SliderActivationChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class SliderActivationChangeCommandHandler : IRequestHandler<SliderActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly ISliderValidator _validator;

        public SliderActivationChangeCommandHandler(ISliderRepository sliderRepository,ISliderValidator validator)
        {
            _sliderRepository = sliderRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(SliderActivationChangeCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _validator.ValidateActivationChangeAsync(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت اسلایدر
                var slider = await _sliderRepository.GetByIdAsync(request.Id);

                // تغییر وضعیت فعالسازی
                slider.ActivationChange();

                // ذخیره تغییرات
                var saveResult = await _sliderRepository.SaveChangesAsync(cancellationToken);
                if (!saveResult)
                {
                    return Error.Failure(
                        "Slider.SaveFailed",
                        "تغییر وضعیت فعالسازی اسلایدر با شکست مواجه شد");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "Slider.ActivationChangeError",
                    $"خطا در تغییر وضعیت فعالسازی اسلایدر: {ex.Message}"
                );
            }
        }
    }
}
