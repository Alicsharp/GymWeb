using ErrorOr;
using Gtm.Contract.SellerContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.SellerApp.Command
{
    public record EditRequestSellerCommand(int userId, EditSellerRequest command) : IRequest<ErrorOr<Success>>;

    public class EditRequestSellerCommandHandler: IRequestHandler<EditRequestSellerCommand, ErrorOr<Success>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IFileService _fileService;
        private readonly ISellerValidator _validator;

        public EditRequestSellerCommandHandler(ISellerRepository sellerRepository,IFileService fileService, ISellerValidator validator)
        {
            _sellerRepository = sellerRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(EditRequestSellerCommand request, CancellationToken cancellationToken)
        {
            // ✅ 1. اعتبارسنجی
            var validation = await _validator.EditSellerValidation(request.userId, request.command);
            if (validation.IsError)
                return validation.Errors;

            var seller = await _sellerRepository.GetByIdAsync(request.command.Id);

            string imageName = seller.ImageName;
            string oldImageName = seller.ImageName;
            string imageAccept = seller.ImageAccept;
            string oldImageAccept = seller.ImageAccept;

            // ✅ 2. آپلود تصویر جدید (در صورت وجود)
            if (request.command.ImageFile != null)
            {
                imageName = await _fileService.UploadImageAsync(request.command.ImageFile, FileDirectories.SellerImageFolder);
                if (string.IsNullOrEmpty(imageName))
                    return Error.Failure("ImageError", ValidationMessages.ImageErrorMessage);

                await _fileService.ResizeImageAsync(imageName, FileDirectories.SellerImageFolder, 500);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.SellerImageFolder, 100);
            }

            if (request.command.ImageAccept != null)
            {
                imageAccept = await _fileService.UploadImageAsync(request.command.ImageAccept, FileDirectories.SellerImageFolder);
                if (string.IsNullOrEmpty(imageAccept))
                    return Error.Failure("ImageError", ValidationMessages.ImageErrorMessage);

                await _fileService.ResizeImageAsync(imageAccept, FileDirectories.SellerImageFolder, 500);
                await _fileService.ResizeImageAsync(imageAccept, FileDirectories.SellerImageFolder, 100);
            }

            // ✅ 3. اعمال تغییرات
            seller.Edit(
                request.command.Title, request.command.StateId, request.command.CityId,
                request.command.Address, request.command.GoogleMapUrl, imageName,
                request.command.ImageAlt, request.command.WhatsApp, request.command.Telegram,
                request.command.Instagram, request.command.Phone1, request.command.Phone2,
                request.command.Email);

            if (request.command.ImageAccept != null)
                seller.EditImageAccept(imageAccept);

            // ✅ 4. ذخیره تغییرات
            if (await _sellerRepository.SaveChangesAsync())
            {
                // پاک کردن عکس‌های قدیمی
                if (request.command.ImageAccept != null)
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{oldImageAccept}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{oldImageAccept}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{oldImageAccept}");
                }

                if (request.command.ImageFile != null)
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{oldImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{oldImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{oldImageName}");
                }

                return Result.Success;
            }

            // اگر ذخیره نشد → پاک کردن تصاویر جدید
            if (request.command.ImageAccept != null)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{imageAccept}");
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{imageAccept}");
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{imageAccept}");
            }

            if (request.command.ImageFile != null)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{imageName}");
            }

            return Error.Failure("SystemError", ValidationMessages.SystemErrorMessage);
        }
    }

}
