using ErrorOr;
using Gtm.Application.SiteServiceApp.SitePageApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SiteServiceApp.SitePageApp.Command
{
    public record ActivationChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class ActivationChangeCommandHandler : IRequestHandler<ActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageValidator _validator;

        public ActivationChangeCommandHandler(ISitePageRepository sitePageRepository,ISitePageValidator validator)
        {
            _sitePageRepository = sitePageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(ActivationChangeCommand request,CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateActivationChangeAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get page
                var page = await _sitePageRepository.GetByIdAsync(request.Id);

                // Toggle activation status
                page.ActivationChange();

                // Save changes
                var result = await _sitePageRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure("SitePage.SaveFailed", "ذخیره تغییرات وضعیت صفحه با خطا مواجه شد.");
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SitePage.OperationFailed",
                    $"خطا در تغییر وضعیت صفحه: {ex.Message}");
            }
        }
    }
}