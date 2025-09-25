 
using Gtm.Application.SiteServiceApp.SitePageApp;
using Gtm.Contract.SiteContract.SitePageContract.Command;
using Gtm.Domain.SiteDomain.SitePageAgg;
using Microsoft.EntityFrameworkCore;
 
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class SitePageRepository : Repository<SitePage, int>, ISitePageRepository
    {
        private readonly GtmDbContext _context;

        public SitePageRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SitePage> GetBySlugAsync(string slug)
        {
            return await _context.SitePages.SingleOrDefaultAsync(s => s.Slug.Trim().ToLower() == slug.Trim().ToLower());
        }

        public async Task<EditSitePage?> GetForEditAsync(int id)
        {
            return await
            _context.SitePages.Select(s => new EditSitePage
            {
                Id = s.Id,
                Slug = s.Slug,
                Text = s.Description,
                Title = s.Title
            }).SingleOrDefaultAsync(s => s.Id == id);
        }

    }
}
