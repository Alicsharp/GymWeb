using ErrorOr;
using Gtm.Contract.SiteContract.SitePageContract.Command;
using MediatR;
using Utility.Appliation.Slug;


namespace Gtm.Application.SiteServiceApp.SitePageApp.Command
{
    public record EditSitePageCommand(EditSitePage Command) : IRequest<ErrorOr<Success>>;

    public class EditSitePageCommandHandler : IRequestHandler<EditSitePageCommand, ErrorOr<Success>>
    {
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageValidator _validator;

        public EditSitePageCommandHandler(ISitePageRepository sitePageRepository,ISitePageValidator validator)
        {
            _sitePageRepository = sitePageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(
            EditSitePageCommand request,
            CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateEditAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get existing page
                var page = await _sitePageRepository.GetByIdAsync(request.Command.Id);
                if (page == null)
                {
                    return Error.NotFound(
                        "SitePage.NotFound",
                        "صفحه مورد نظر یافت نشد.");
                }

                // Generate slug
                var slug = request.Command.Slug.GenerateSlug();

                // Apply changes
                page.Edit(
                    request.Command.Title.Trim(),
                    slug,
                    request.Command.Text);

                // Save changes
                var result = await _sitePageRepository.SaveChangesAsync(cancellationToken);
                if (!result)
                {
                    return Error.Failure(
                        "SitePage.SaveFailed",
                        "ذخیره تغییرات با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SitePage.OperationFailed",
                    $"خطا در ویرایش صفحه: {ex.Message}");
            }
        }
    }
}
