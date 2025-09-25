using ErrorOr;
using Gtm.Application.SiteServiceApp.BannerApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SiteServiceApp.BannerApp.Command
{
    public record ActivationChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class ActivationChangeCommandHandler : IRequestHandler<ActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IBanerRepository _banerRepository;
        private readonly IBannerValidator _bannerValidator;

        public ActivationChangeCommandHandler(
            IBanerRepository banerRepository,
            IBannerValidator bannerValidator)
        {
            _banerRepository = banerRepository;
            _bannerValidator = bannerValidator;
        }

        public async Task<ErrorOr<Success>> Handle(ActivationChangeCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _bannerValidator.ValidateActivationChangeAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // انجام عملیات
            var banner = await _banerRepository.GetByIdAsync(request.Id);
            banner.ActivationChange();

            // ذخیره تغییرات
            var result = await _banerRepository.SaveChangesAsync(cancellationToken);

            if (!result)
                return Error.Failure(description: "ذخیره تغییرات با خطا مواجه شد.");

            return Result.Success;
        }
    }
}
