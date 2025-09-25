 
using Gtm.Application.SiteServiceApp.SiteSettingApp;
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using Gtm.Domain.SiteDomain.SiteSettingAgg;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SiteRepo
{
    internal class SiteSettingRepository : Repository<SiteSetting,int>, ISiteSettingRepository
    {
        private readonly GtmDbContext _context;

        public SiteSettingRepository(GtmDbContext context):base(context) 
        {
            _context = context;
        }

        public async Task<UbsertSiteSetting> GetForUbsertAsync()
        {
            var site = await GetSingleAsync();
            return new()
            {
                AboutDescription = site.AboutDescription,
                AboutTitle = site.AboutTitle,
                Address = site.Address,
                Android = site.Android,
                LogoAlt = site.LogoAlt,
                WhatsApp = site.WhatsApp,
                ContactDescription = site.ContactDescription,
                Email1 = site.Email1,
                Email2 = site.Email2,
                Enamad = site.Enamad,
                FavIcon = site.FavIcon,
                FavIconFile = null,
                FooterDescription = site.FooterDescription,
                FooterTitle = site.FooterTitle,
                Instagram = site.Instagram,
                IOS = site.IOS,
                LogoFile = null,
                LogoName = site.LogoName,
                Phone1 = site.Phone1,
                Phone2 = site.Phone2,
                SamanDehi = site.SamanDehi,
                SeoBox = site.SeoBox,
                Telegram = site.Telegram,
                Youtube = site.Youtube
            };
        }

        public async Task<SiteSetting> GetSingleAsync()
        {
            SiteSetting site = await _context.SiteSettings.SingleOrDefaultAsync();
            if (site == null)
            {
                site = new();
                _context.SiteSettings.Add(site);
                await SaveAsync();

            }
            return site;
        }
        public async Task<bool> SaveAsync() { return await _context.SaveChangesAsync() >= 0 ? true : false; }

    }
}
