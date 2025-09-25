using ErrorOr;
 using MediatR;

namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Command
{
    public record ActivationChangeSiteServiceCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class ActivationChangeSiteServiceCommandHandler: IRequestHandler<ActivationChangeSiteServiceCommand, ErrorOr<Success>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;
        private readonly ISiteServiceValidator _validator;

        public ActivationChangeSiteServiceCommandHandler(ISiteServiceRepository siteServiceRepository,ISiteServiceValidator validator)
        {
            _siteServiceRepository = siteServiceRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(ActivationChangeSiteServiceCommand request,CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateActivationChangeAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get service
                var service = await _siteServiceRepository.GetByIdAsync(request.Id);

                // Toggle activation status
                service.ActivationChange();

                // Save changes
                var result = await _siteServiceRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure("SiteService.SaveFailed", "ذخیره تغییرات وضعیت سرویس با خطا مواجه شد.");
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SiteService.OperationFailed",
                    $"خطا در تغییر وضعیت سرویس: {ex.Message}");
            }
        }
    }
}
