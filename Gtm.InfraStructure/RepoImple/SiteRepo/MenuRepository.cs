using Gtm.Application.SiteServiceApp.MenuApp;
using Gtm.Contract.SiteContract.MenuContract.Command;
using Gtm.Domain.SiteDomain.MenuAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class MenuRepository : Repository<Menu,int>, IMenuRepository
    {
        private readonly GtmDbContext _context;

        public MenuRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EditMenu?> GetForEditAsync(int id)
        {
            return await _context.Menus.Select(s => new EditMenu
            {
                ImageAlt = s.ImageAlt,
                Id = s.Id,
                ImageFile = null,
                ImageName = s.ImageName,
                Number = s.Number,
                Title = s.Title,
                ParentId = s.ParentId == null ? 0 : s.ParentId.Value,
                Url = s.Url
            }).SingleOrDefaultAsync(s => s.Id == id);
        }
    }
}
