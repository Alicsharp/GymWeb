
using Gtm.Application.SiteServiceApp.SliderApp;
using Gtm.Contract.SiteContract.SliderContract.Command;
using Gtm.Domain.SiteDomain.SliderAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;


namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class SliderRepository : Repository<Slider,int>, ISliderRepository
    {
        private readonly GtmDbContext _context;

        public SliderRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<EditSlider?> GetForEditAsync(int id)
        {
            return await _context.Sliders.Select(s => new EditSlider
            {
                ImageAlt = s.ImageAlt,
                Id = s.Id,
                ImageFile = null,
                ImageName = s.ImageName,
                Url = s.Url
            }).SingleOrDefaultAsync(s => s.Id == id);
        }

    }
}
