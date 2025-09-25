using ErrorOr;
using Gtm.Contract.SiteContract.SitePageContract.Command;
using MediatR;
 
namespace Gtm.Application.SiteServiceApp.SitePageApp.Query
{
    public record GetForEditQuery(int Id) : IRequest<ErrorOr<EditSitePage>>;

    public class GetForEditQueryHandler : IRequestHandler<GetForEditQuery, ErrorOr<EditSitePage>>
    {
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageValidator _validator;

        public GetForEditQueryHandler(ISitePageRepository sitePageRepository,ISitePageValidator validator)
        {
            _sitePageRepository = sitePageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EditSitePage>> Handle(GetForEditQuery request,CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForEditAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get page for edit
                var editPage = await _sitePageRepository.GetForEditAsync(request.Id);

                // Additional null check (even though validator checked existence)
                if (editPage == null)
                {
                    return Error.NotFound(
                        "SitePage.NotFound",
                        "صفحه با این شناسه یافت نشد.");
                }

                return editPage;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SitePage.RetrievalError",
                    $"خطا در دریافت اطلاعات صفحه: {ex.Message}");
            }
        }
    }
}
