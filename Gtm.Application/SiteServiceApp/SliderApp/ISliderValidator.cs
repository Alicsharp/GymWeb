using ErrorOr;
using Gtm.Contract.SiteContract.SliderContract.Command;
using Utility.Appliation.FileService;
using Utility.Appliation;

namespace Gtm.Application.SiteServiceApp.SliderApp
{
       public interface ISliderValidator
        {
           Task<ErrorOr<Success>> ValidateCreateAsync(CreateSlider command);
           Task<ErrorOr<Success>> ValidateEditAsync(EditSlider command);
           Task<ErrorOr<Success>> ValidateActivationChangeAsync(int sliderId);
         Task<ErrorOr<Success>> ValidateGetForEditAsync(int sliderId);
       }

         public class SliderValidator : ISliderValidator
         {
          private readonly IFileService _fileService;
          private readonly ISliderRepository _sliderRepository;

         public SliderValidator(IFileService fileService, ISliderRepository sliderRepository)
         {
            _fileService = fileService;
            _sliderRepository = sliderRepository;
        }
 
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateSlider command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی تصویر اسلایدر
            if (command.ImageFile == null)
            {
                errors.Add(Error.Validation("Slider.ImageRequired", "تصویر اسلایدر الزامی است"));
            }
            else
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("Slider.InvalidImage", "فایل ارسالی باید یک تصویر معتبر باشد"));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Slider.ImageTooLarge", "حجم تصویر نباید بیشتر از 5 مگابایت باشد"));
                }
            }

            // اعتبارسنجی متن جایگزین تصویر
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation("Slider.ImageAltRequired", "متن جایگزین تصویر الزامی است"));
            }
            else if (command.ImageAlt.Length > 100)
            {
                errors.Add(Error.Validation("Slider.ImageAltTooLong", "متن جایگزین تصویر نباید بیشتر از 100 کاراکتر باشد"));
            }

            // اعتبارسنجی لینک اسلایدر
            if (!string.IsNullOrWhiteSpace(command.Url) &&
                !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("Slider.InvalidUrl", "لینک اسلایدر معتبر نیست"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditSlider command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی وجود اسلایدر
            var slider = await _sliderRepository.GetByIdAsync(command.Id);
            if (slider == null)
            {
                errors.Add(Error.NotFound("Slider.NotFound", "اسلایدر مورد نظر یافت نشد"));
                return errors;
            }

            // اعتبارسنجی تصویر (اگر ارسال شده)
            if (command.ImageFile != null)
            {
                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("Slider.InvalidImage", "فایل ارسالی باید یک تصویر معتبر باشد"));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Slider.ImageTooLarge", "حجم تصویر نباید بیشتر از 5 مگابایت باشد"));
                }
            }

            // اعتبارسنجی متن جایگزین تصویر
            if (string.IsNullOrWhiteSpace(command.ImageAlt))
            {
                errors.Add(Error.Validation("Slider.ImageAltRequired", "متن جایگزین تصویر الزامی است"));
            }
            else if (command.ImageAlt.Length > 100)
            {
                errors.Add(Error.Validation("Slider.ImageAltTooLong", "متن جایگزین تصویر نباید بیشتر از 100 کاراکتر باشد"));
            }

            // اعتبارسنجی لینک اسلایدر
            if (!string.IsNullOrWhiteSpace(command.Url) &&
                !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("Slider.InvalidUrl", "لینک اسلایدر معتبر نیست"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int sliderId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه اسلایدر
            if (sliderId <= 0)
            {
                errors.Add(Error.Validation("Slider.InvalidId", "شناسه اسلایدر نامعتبر است"));
            }

            // اعتبارسنجی وجود اسلایدر
            if (sliderId > 0)
            {
                var sliderExists = await _sliderRepository.ExistsAsync(c=>c.Id==sliderId);
                if (!sliderExists)
                {
                    errors.Add(Error.NotFound("Slider.NotFound", "اسلایدر مورد نظر یافت نشد"));
                }
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int sliderId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه اسلایدر
            if (sliderId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Slider.InvalidId",
                    description: "شناسه اسلایدر نامعتبر است"));
            }

            // اعتبارسنجی وجود اسلایدر
            if (sliderId > 0 && !await _sliderRepository.ExistsAsync(c=>c.Id==sliderId))
            {
                errors.Add(Error.NotFound(
                    code: "Slider.NotFound",
                    description: "اسلایدر مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
 

 
