using Gtm.Contract.SiteContract.SliderContract.Command;
using Gtm.Domain.SiteDomain.SliderAgg;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.SiteServiceApp.SliderApp
{
    public interface ISliderRepository : IRepository<Slider, int>
    {
        Task<EditSlider?> GetForEditAsync(int id);
    }
}




