using ErrorOr;
 
using Gtm.Contract.SiteContract.SitePageContract.Command;
using Gtm.Domain.SiteDomain.SitePageAgg;
using MediatR;
using Utility.Appliation.Slug;

namespace Gtm.Application.SiteServiceApp.SitePageApp.Command
{
    public record CreateSitePageCommand(CreateSitePage Command) : IRequest<ErrorOr<Success>>;

    public class CreateSitePageCommandHandler : IRequestHandler<CreateSitePageCommand, ErrorOr<Success>>
    {
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageValidator _validator;

        public CreateSitePageCommandHandler(ISitePageRepository sitePageRepository,ISitePageValidator validator)
        {
            _sitePageRepository = sitePageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateSitePageCommand request,CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Generate slug
                var slug = request.Command.Slug.GenerateSlug();

                // Create new page
                var page = new SitePage(
                    request.Command.Title.Trim(),
                    slug,
                    request.Command.Text);

                // Save to database
                await _sitePageRepository.AddAsync(page);
                var result =await _sitePageRepository.SaveChangesAsync(cancellationToken);
                if (!result)
                {
                    return Error.Failure(
                        "SitePage.CreationFailed",
                        "خطا در ایجاد صفحه جدید.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SitePage.OperationFailed",
                    $"خطا در ایجاد صفحه: {ex.Message}");
            }
        }
    }
}
