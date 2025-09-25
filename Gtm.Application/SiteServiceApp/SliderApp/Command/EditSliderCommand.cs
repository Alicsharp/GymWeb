using ErrorOr;
using Gtm.Contract.SiteContract.SliderContract.Command;
using Gtm.Domain.SiteDomain.SliderAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.SliderApp.Command
{
    public record EditSliderCommand(EditSlider Command) : IRequest<ErrorOr<Success>>;

    public class EditSliderCommandHandler : IRequestHandler<EditSliderCommand, ErrorOr<Success>>
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly IFileService _fileService;
        private readonly ISliderValidator _validator;

        public EditSliderCommandHandler(ISliderRepository sliderRepository,IFileService fileService,ISliderValidator validator)
        {
            _sliderRepository = sliderRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(EditSliderCommand request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateEditAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            string oldImageName = string.Empty;
            string newImageName = string.Empty;

            try
            {
                // دریافت اسلایدر موجود
                var slider = await _sliderRepository.GetByIdAsync(request.Command.Id);
                oldImageName = slider.ImageName;
                newImageName = oldImageName;

                // مدیریت آپدیت تصویر (اگر فایل جدید ارسال شده)
                if (request.Command.ImageFile != null)
                {
                    // آپلود تصویر جدید
                    newImageName = await _fileService.UploadImageAsync(
                        request.Command.ImageFile,
                        FileDirectories.SliderImageFolder);

                    if (string.IsNullOrEmpty(newImageName))
                    {
                        return Error.Failure(
                            "Slider.ImageUploadFailed",
                            "آپلود تصویر جدید با شکست مواجه شد");
                    }

                    // تغییر سایز تصویر جدید
                    var resizeResult = await _fileService.ResizeImageAsync(
                        newImageName,
                        FileDirectories.SliderImageFolder,
                        100);

                    if (!resizeResult)
                    {
                        await RollbackNewImages(newImageName);
                        return Error.Failure(
                            "Slider.ImageResizeFailed",
                            "تغییر سایز تصویر جدید با شکست مواجه شد");
                    }
                }

                // آپدیت اسلایدر
                slider.Edit(
                    newImageName,
                    request.Command.ImageAlt,
                    request.Command.Url);

                // ذخیره تغییرات
                var saveResult = await _sliderRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    await RollbackChanges(slider, newImageName, oldImageName);
                    return Error.Failure(
                        "Slider.UpdateFailed",
                        "بروزرسانی اسلایدر در پایگاه داده با شکست مواجه شد");
                }

                // حذف تصاویر قدیمی اگر تصویر جدید آپلود شده بود
                if (request.Command.ImageFile != null)
                {
                    await DeleteOldImages(oldImageName);
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackChanges(null, newImageName, oldImageName);
                return Error.Failure(
                    "Slider.UnexpectedError",
                    $"خطای غیرمنتظره در ویرایش اسلایدر: {ex.Message}");
            }
        }

        private async Task RollbackChanges(Slider? slider, string newImageName, string oldImageName)
        {
            try
            {
                // حذف تصاویر جدید در صورت خطا
                if (!string.IsNullOrEmpty(newImageName) && newImageName != oldImageName)
                {
                    await RollbackNewImages(newImageName);
                }

                // بازگرداندن تصویر قدیمی در مدل اگر تغییر کرده بود
                if (slider != null && slider.ImageName != oldImageName)
                {
                    slider.Edit(oldImageName, slider.ImageAlt, slider.Url);
                }
            }
            catch
            {
                // Log error if needed
            }
        }

        private async Task RollbackNewImages(string imageName)
        {
            await Task.WhenAll(
                _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory}{imageName}"),
                _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory100}{imageName}")
            );
        }

        private async Task DeleteOldImages(string imageName)
        {
            await Task.WhenAll(
                _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory}{imageName}"),
                _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory100}{imageName}")
            );
        }
    }
}
