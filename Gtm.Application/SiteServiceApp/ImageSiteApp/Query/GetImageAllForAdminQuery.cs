using ErrorOr;

using Gtm.Contract.SiteContract.ImageSiteContract.Query;
using Gtm.Domain.SiteDomain.SiteImageAgg;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.ImageSiteApp.Query
{
    public record GetImageAllForAdminQuery(int PageId, int Take, string Filter): IRequest<ErrorOr<ImageAdminPaging>>;

    public class GetImageAllForAdminQueryHandler : IRequestHandler<GetImageAllForAdminQuery, ErrorOr<ImageAdminPaging>>
    {
        private readonly IImageSiteRepository _imageSiteRepository;
        private readonly IImageSiteValidator _validator;

        public GetImageAllForAdminQueryHandler(IImageSiteRepository imageSiteRepository, IImageSiteValidator validator)
        {
            _imageSiteRepository = imageSiteRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<ImageAdminPaging>> Handle(GetImageAllForAdminQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = _validator.ValidateGetAllForAdmin(request.PageId,request.Take,request.Filter);

            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Create base query
                var query = _imageSiteRepository.GetAllQueryable();

                // Apply filter if exists
                if (!string.IsNullOrWhiteSpace(request.Filter))
                    query = query.Where(x => x.Title.Contains(request.Filter));

                // Create paging model
                var model = new ImageAdminPaging
                {
                    Filter = request.Filter,
                    Images = new List<ImageSiteAdminQueryModel>()
                };

                // Calculate paging data
                model.GetData(query, request.PageId, request.Take, 5);

                // Get paginated data
                model.Images = await query
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .Select(x => new ImageSiteAdminQueryModel
                    {
                        CreateDate = x.CreateDate.ToPersainDate(),
                        Id = x.Id,
                        ImageName =  x.ImageName,
                        Title = x.Title
                    })
                    .ToListAsync(cancellationToken);

                return model;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "Image.QueryFailed",
                    $"خطا در دریافت لیست تصاویر: {ex.Message}");
            }

        }
    }
}
