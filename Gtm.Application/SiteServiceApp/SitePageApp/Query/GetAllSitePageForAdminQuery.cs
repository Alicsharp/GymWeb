using ErrorOr;
using Gtm.Contract.SiteContract.SitePageContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;


namespace Gtm.Application.SiteServiceApp.SitePageApp.Query
{
    public record GetAllSitePageForAdminQuery : IRequest<ErrorOr<List<SitePageAdminQueryModel>>>;

    public class GetAllSitePageForAdminQueryHandler: IRequestHandler<GetAllSitePageForAdminQuery, ErrorOr<List<SitePageAdminQueryModel>>>
    {
        private readonly ISitePageRepository _sitePageRepository;

        public GetAllSitePageForAdminQueryHandler(ISitePageRepository sitePageRepository)
        {
            _sitePageRepository = sitePageRepository;
        }

        public async Task<ErrorOr<List<SitePageAdminQueryModel>>> Handle(GetAllSitePageForAdminQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // Use GetAllQueryable() for better query composition
                var result = await _sitePageRepository.GetAllQueryable()
                    .Select(p => new SitePageAdminQueryModel
                    {
                        Active = p.IsActive,
                        CreateDate = p.CreateDate.ToPersainDate(),
                        Id = p.Id,
                        Slug = p.Slug,
                        Title = p.Title,
                        UpdateDate = p.UpdateDate.ToPersainDate()
                    })
                    .ToListAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SitePage.QueryFailed",
                    $"خطا در دریافت لیست صفحات: {ex.Message}");
            }
        }
    }
}