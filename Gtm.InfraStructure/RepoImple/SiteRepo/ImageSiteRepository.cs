 
using Gtm.Application.SiteServiceApp.ImageSiteApp;
using Gtm.Domain.SiteDomain.SiteImageAgg;
 
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class ImageSiteRepository : Repository<SiteImage,int>, IImageSiteRepository
    {
        private readonly GtmDbContext _context;

        public ImageSiteRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
