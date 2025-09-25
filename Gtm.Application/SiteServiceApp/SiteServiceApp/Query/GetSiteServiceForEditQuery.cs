using ErrorOr;
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using MediatR;
 
namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Query
{
    public record GetSiteServiceForEditQuery(int Id) : IRequest<ErrorOr<EditSiteService>>;

    public class GetSiteServiceForEditQueryHandler : IRequestHandler<GetSiteServiceForEditQuery, ErrorOr<EditSiteService>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;
        private readonly ISiteServiceValidator _validator;

        public GetSiteServiceForEditQueryHandler(ISiteServiceRepository siteServiceRepository,ISiteServiceValidator validator)
        {
            _siteServiceRepository = siteServiceRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EditSiteService>> Handle(
            GetSiteServiceForEditQuery request,
            CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForEditAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get service for edit
                var service = await _siteServiceRepository.GetForEditAsync(request.Id);

                // Additional null check (even though validator checked existence)
                if (service == null)
                {
                    return Error.NotFound(
                        "SiteService.NotFound",
                        "سرویس با این شناسه یافت نشد.");
                }

                return service;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SiteService.RetrievalError",
                    $"خطا در دریافت اطلاعات سرویس: {ex.Message}");
            }
        }
    }
}
