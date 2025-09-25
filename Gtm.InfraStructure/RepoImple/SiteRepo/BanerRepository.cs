
using Gtm.Application.SiteServiceApp.BannerApp;
using Gtm.Contract.SiteContract.BanarContract.Command;
using Gtm.Domain.SiteDomain.BannerAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;


namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    public class BanerRepository : Repository<Baner, int>, IBanerRepository
    {
        private readonly GtmDbContext _context;

        public BanerRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditBaner?> GetForEditAsync(int id)
        {
            return await _context.Baners.Where(s => s.Id == id).Select(s => new EditBaner
            {
                ImageAlt = s.ImageAlt,
                Id = s.Id,
                ImageFile = null, // فرض بر این است که ImageFile در حال حاضر null است
                ImageName = s.ImageName,
                Url = s.Url
            }).SingleOrDefaultAsync();
        }
    }
}
