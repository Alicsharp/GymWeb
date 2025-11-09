using Gtm.Application.ShopApp.SellerApp;
using Gtm.Application.SiteServiceApp.SliderApp;
using Gtm.Contract.SellerContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.SellerRepo
{
    public class SellerRepository : Repository<Seller, int>, ISellerRepository
    {
        private readonly GtmDbContext _context;

        public SellerRepository(GtmDbContext context):base(context) 
        {
            _context = context;
        }
        public async Task<Seller?> GetSellerByIdAsync(int id)
        {
            return await _context.Sellers.SingleOrDefaultAsync(s => s.Id == id);
        }
        public async Task<SellerRequestDetailAdminQueryModel> GetSellerRequestDetailForAdmin(int id)
        {
            var s = _context.Sellers.SingleOrDefault(s => s.Id == id);
            if (s == null) return null;
            SellerRequestDetailAdminQueryModel model = new()
            {
                Address = s.Address,
                ImageAccept = s.ImageAccept,
                ImageAlt = s.ImageAlt,
                WhatsApp = s.WhatsApp,
                CityId = s.CityId,
                CityName = "",
                CreateDate = s.CreateDate.ToPersainDate(),
                Email = s.Email,
                GoogleMapUrl = s.GoogleMapUrl,
                Id = id,
                ImageName = s.ImageName,
                Instagram = s.Instagram,
                Phone1 = s.Phone1,
                Phone2 = s.Phone2,
                StateId = s.StateId,
                Status = s.Status,
                Telegram = s.Telegram,
                Title = s.Title,
                UpdateDate = s.UpdateDate.ToPersainDate(),
                UserId = s.UserId,
                UserName = ""
            };

            var user = _context.Users.SingleOrDefault(u => u.Id == s.UserId);
            if (user == null) return null;
            model.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
            var state = _context.States.SingleOrDefault(c => c.Id == s.StateId);
            if (state == null) return null;
            var city = _context.Cities.SingleOrDefault(c => c.Id == s.CityId);
            if (city == null) return null;
            model.CityName = $"{state.Title} {city.Title}";
            return model;
        }
        public async Task<List<SellerRequestAdminQueryModel>> GetSellerRequestsForAdmin()
        {

            var res = _context.Sellers.Where(s => s.Status == SellerStatus.درخواست_ارسال_شده);
            var model = res.Select(s => new SellerRequestAdminQueryModel
            {
                ImageAccept = s.ImageAccept,
                CityId = s.CityId,
                CityName = "",
                CreateDate = s.CreateDate.ToPersainDate(),
                Email = s.Email,
                Id = s.Id,
                ImageName = s.ImageName,
                Phone1 = s.Phone1,
                StateId = s.StateId,
                Title = s.Title,
                UpdateDate = s.UpdateDate.ToPersainDate(),
                UserId = s.UserId,
                UserName = ""
            }).ToList();
            model.ForEach(s =>
            {
                var user = _context.Users.SingleOrDefault(u => u.Id == s.UserId);
                if (user != null)
                    s.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                var state = _context.States.SingleOrDefault(c => c.Id == s.StateId);
                if (state != null)
                    s.CityName = state.Title;
                var city = _context.Cities.SingleOrDefault(c => c.Id == s.CityId);
                if (city != null)
                    s.CityName = s.CityName + " " + city.Title;
            });
            return model;
        }
        public async Task<Seller?> GetSellerForUserPanelAsync(int id, int userId)
        {
            return await _context.Sellers
                .SingleOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        }
        public async Task<bool> IsSellerForUserAsync(int sellerId, int userId, CancellationToken cancellationToken = default)
        {
            // به جای Find و سپس بررسی، مستقیماً از AnyAsync استفاده می‌کنیم
            return await _context.Sellers
                .AnyAsync(s => s.Id == sellerId && s.UserId == userId, cancellationToken);
        }
    }
}
