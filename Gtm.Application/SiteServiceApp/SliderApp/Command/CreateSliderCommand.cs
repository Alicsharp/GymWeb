using ErrorOr;
using Gtm.Contract.SiteContract.SliderContract.Command;
using Gtm.Domain.SiteDomain.SliderAgg;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;



namespace Gtm.Application.SiteServiceApp.SliderApp.Command
{
    public record CreateSliderCommand(CreateSlider Command) : IRequest<ErrorOr<Success>>;

    public class CreateSliderCommandHandler : IRequestHandler<CreateSliderCommand, ErrorOr<Success>>
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly IFileService _fileService;
        private readonly ISliderValidator _validator;

        public CreateSliderCommandHandler(ISliderRepository sliderRepository,IFileService fileService,ISliderValidator validator)
        {
            _sliderRepository = sliderRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateSliderCommand request,CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            string imageName = string.Empty;
            try
            {
                // Upload image
                imageName = await _fileService.UploadImageAsync(
                    request.Command.ImageFile,
                    FileDirectories.SliderImageFolder);

                if (string.IsNullOrEmpty(imageName))
                {
                    return Error.Failure(
                        "Slider.UploadFailed",
                        "آپلود تصویر اسلایدر با خطا مواجه شد.");
                }

                // Resize image
                await _fileService.ResizeImageAsync(imageName, FileDirectories.SliderImageFolder, 100);

                // Create slider
                var slider = new Slider(
                    imageName,
                    request.Command.ImageAlt,
                    request.Command.Url);

                // Save to database
                await _sliderRepository.AddAsync(slider);
                var saveResult = await _sliderRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    await RollbackImageOperations(imageName);
                    return Error.Failure(
                        "Slider.SaveFailed",
                        "ذخیره اسلایدر در پایگاه داده با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                await RollbackImageOperations(imageName);
                return Error.Failure(
                    "Slider.OperationFailed",
                    $"خطا در ایجاد اسلایدر: {ex.Message}");
            }
        }

        private async Task RollbackImageOperations(string imageName)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageName))
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory}{imageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.SliderImageDirectory100}{imageName}");
                }
            }
            catch
            {
                // Log error if needed
            }
        }
    }
}
