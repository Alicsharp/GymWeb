
using Gtm.Application.SiteServiceApp.SiteServiceApp;
using Gtm.Contract.SiteContract.SiteServiceContract.Command;
using Gtm.Domain.SiteDomain.SiteServiceAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;


namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class SiteServiceRepository : Repository<SiteService, int>, ISiteServiceRepository
    {
        private readonly GtmDbContext _context;

        public SiteServiceRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditSiteService> GetForEditAsync(int id)
        {
            return await _context.SiteServices.Select(s => new EditSiteService
            {
                ImageAlt = s.ImageAlt,
                Id = s.Id,
                ImageFile = null,
                ImageName = s.ImageName,
                Title = s.Title
            }).SingleOrDefaultAsync(s => s.Id == id);
        }

    }
}
