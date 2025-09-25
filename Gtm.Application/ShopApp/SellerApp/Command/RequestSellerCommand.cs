using ErrorOr;
using Gtm.Contract.SellerContract.Command;
using Gtm.Domain.ShopDomain.SellerDomain;
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
    public record RequestSellerCommand(int userId, RequestSeller command) : IRequest<ErrorOr<Success>>;
    public class RequestSellerCommandHandler : IRequestHandler<RequestSellerCommand, ErrorOr<Success>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IFileService _fileService;
        private readonly ISellerValidator _validator;

        public RequestSellerCommandHandler(ISellerRepository sellerRepository, IFileService fileService, ISellerValidator validator)
        {
            _sellerRepository = sellerRepository;
            _fileService = fileService;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(RequestSellerCommand request, CancellationToken cancellationToken)
        {
            // ✅ اول اعتبارسنجی
            var validation = await _validator.ValidateRequestSellerAsync(request.userId,request.command);
            if (validation.IsError)
                return validation.Errors;

            // ✅ ادامه همان منطق آپلود
            string imageName = await _fileService.UploadImageAsync(request.command.ImageFile, FileDirectories.SellerImageFolder);
            if (string.IsNullOrEmpty(imageName))
                return Error.Failure("ImageError", ValidationMessages.ImageErrorMessage);

            await _fileService.ResizeImageAsync(imageName, FileDirectories.SellerImageFolder, 500);
            await _fileService.ResizeImageAsync(imageName, FileDirectories.SellerImageFolder, 100);

            string imageAccept = await _fileService.UploadImageAsync(request.command.ImageAccept, FileDirectories.SellerImageFolder);
            if (string.IsNullOrEmpty(imageAccept))
                return Error.Failure("ImageError", ValidationMessages.ImageErrorMessage);

            await _fileService.ResizeImageAsync(imageAccept, FileDirectories.SellerImageFolder, 500);
            await _fileService.ResizeImageAsync(imageAccept, FileDirectories.SellerImageFolder, 100);

            Seller seller = new Seller(
                request.userId, request.command.Title, request.command.StateId, request.command.CityId,
                request.command.Address, request.command.GoogleMapUrl, imageAccept, imageName,
                request.command.ImageAlt, request.command.WhatsApp, request.command.Telegram,
                request.command.Instagram, request.command.Phone1, request.command.Phone2,
                request.command.Email);
            
            await _sellerRepository.AddAsync(seller);
            var result = await _sellerRepository.SaveChangesAsync();
            if (result == true)
                return Result.Success;

            // اگر خطا خورد، فایل‌ها پاک بشن
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{imageName}");
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{imageName}");
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{imageName}");
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory}{imageAccept}");
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory500}{imageAccept}");
            await _fileService.DeleteImageAsync($"{FileDirectories.SellerImageDirectory100}{imageAccept}");

            return Error.Failure("system error", ValidationMessages.SystemErrorMessage);
        }
    }

}
