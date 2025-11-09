using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Contract.PostContract.CityContract.Command;
using Gtm.Contract.PostContract.CityContract.Query;
using Gtm.Domain.PostDomain.CityAgg;
using Gtm.Domain.PostDomain.StateAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class CityRepository : Repository<City, int>, ICityRepo
    {
        private readonly GtmDbContext _context;
        public CityRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ChangeStatusAsync(int id, CityStatus status)
        {
            var city = await GetByIdAsync(id);

            List<City> cities = new();
            if (status == CityStatus.تهران)
            {
                cities = _context.Cities.Where(c => c.Status == CityStatus.تهران).ToList();
            }
            else if (status == CityStatus.مرکز_استان)
            {
                cities = _context.Cities.Where(c => c.Status == CityStatus.مرکز_استان && c.StateId == city.StateId).ToList();
            }
            city.ChangeStatus(status);

            if (cities.Count() > 0)
                foreach (var item in cities)
                {
                    item.ChangeStatus(CityStatus.شهرستان_معمولی);
                }
       return true;
        }

        public async Task<List<CityViewModel>> GetAllForStateAsync(int stateId)
        {
            var cities = await GetAllByQueryAsync(c => c.StateId == stateId);
            return cities.Select(c => new CityViewModel
            {
                CreateDate = c.CreateDate.ToString(),
                Id = c.Id,
                Status = c.Status,
                Title = c.Title
            }).ToList(); // ToList() همگام است، اما مشکلی ندارد چون عملیات سنگین نیست.
        }

        public async Task<EditCityModel> GetCityForEditAsync(int id)
        {
            var city = await GetByIdAsync(id);
            return new()
            {
                Id = city.Id,
                Status = city.Status,
                Title = city.Title
            };
        }

        public async Task<City?> GetCityForSellerAsync(Expression<Func<City, bool>> sellerCondition)
        {
            return await GetCityWithStateAsync(sellerCondition);
        }

        public async Task<City?> GetCityWithStateAsync(Expression<Func<City, bool>> predicate)
        {
            return await _context.Cities
                .Include(c => c.State)
                .AsNoTracking() // اگر نیاز به ویرایش نیست
                .SingleOrDefaultAsync(predicate);
        }

        public async Task<City?> GetCityWithStateByIdAsync(int Id)
        {
            return await _context.Cities
               .Include(c => c.State)
        .SingleOrDefaultAsync(c => c.Id == Id );
        }

        public async Task<City?> GetCityWithStateForSellerAsync(int cityId, int stateId)
        {
            return await _context.Cities
               .Include(c => c.State)
        .SingleOrDefaultAsync(c => c.Id == cityId && c.StateId == stateId);
        }
        public async Task<int> GetCityOfSeller(int sellerId)
        {
            var seller = await _context.Sellers
                .SingleOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null)
                return 0;

            return seller.CityId;
        }
        public async Task<List<City>> GetCitiesWithStateAsync(IEnumerable<int> cityIds)
        {
            return await _context.Cities
                .Include(c => c.State)
                .Where(c => cityIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<City> GetCityWithStateAsync(int CityId, int StateId)
        {
            // منطق فیلتر دقیقاً همان چیزی است که در مثال خود استفاده کردید
            return await _context.Cities
                .Include(c => c.State)
                .AsNoTracking() // برای بهینه‌سازی (اگر فقط قصد خواندن دارید)
                .SingleAsync(c => c.Id == CityId && c.StateId == StateId);
        }
    }
}
