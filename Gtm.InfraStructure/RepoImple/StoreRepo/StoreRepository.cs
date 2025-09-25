using Gtm.Application.StoresServiceApp.StroreApp;
using Gtm.Contract.StoresContract.StoreContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.StoresDomain.StoreAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.StoreRepo
{
    internal class StoreRepository : Repository<Store,int>, IStoreRepository
    {
        private readonly GtmDbContext _context;
        public StoreRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public   IQueryable<Store> GetStoresByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _context.Stores.Where(s => s.UserId == userId);
        }

        public async Task<StoreUserPanelPaging> GetStoresForUserPanelAsync(int userId, int sellerId, string filter, int pageId = 1, int take = 10)
        {
            var query = _context.Stores
       .Where(s => s.UserId == userId)
       .AsQueryable(); // ابتدا به IQueryable تبدیل کنید

            // فیلترها را اعمال کنید
            if (sellerId > 0)
                query = query.Where(s => s.SellerId == sellerId);

            if (!string.IsNullOrEmpty(filter))
                query = query.Where(r => r.Description.Contains(filter));

            // در انتها مرتب سازی کنید
            var orderedQuery = query.OrderByDescending(o => o.Id);

            StoreUserPanelPaging model = new StoreUserPanelPaging();
            model.GetData(query, pageId, take, 2);
            model.Filter = filter;
            model.SellerId = sellerId;
            model.PageTitle = "انبار داری";
            model.Stores = new List<StoreUserPanelQueryModel>();

            var totalCount = await query.CountAsync();
            if (totalCount > 0)
            {
                var stores = await query
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .Select(s => new StoreUserPanelQueryModel
                    {
                        CreationDate = s.CreateDate.ToPersainDate(),
                        Id = s.Id,
                        SellerId = s.SellerId,
                        SellerName = ""
                    })
                    .ToListAsync();

                // پر کردن نام فروشگاه‌ها
                var sellerIds = stores.Select(s => s.SellerId).Distinct();
                var sellers = await _context.Sellers
                    .Where(s => sellerIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s.Title);

                foreach (var store in stores)
                {
                    if (sellers.TryGetValue(store.SellerId, out var sellerName))
                    {
                        store.SellerName = sellerName;
                    }
                }

                model.Stores = stores;
            }

            if (model.SellerId > 0)
            {
                var seller = await _context.Sellers.FindAsync(model.SellerId);
                if (seller == null || seller.UserId != userId)
                    return null;

                model.PageTitle = $"انبار داری فروشگاه {seller.Title}";
            }

            return model;
        }
        //public async Task<int> CreateReturnKey(Store store)
        //{
        // await  _context.Stores.AddAsync (store);
        //    await _context.SaveChangesAsync( );
        //    return store.Id;
        //}
        public async Task<int> CreateReturnKey(Store store)
        {
            await _context.Stores.AddAsync(store);
           _context.SaveChanges();
            return store.Id;
        }

        public async Task<Store?> GetStoreWithProductsAsync(int storeId)
        {
            return await _context.Stores
                .Include(s => s.StoreProducts)
                .SingleOrDefaultAsync(s => s.Id == storeId);
        }

        public async Task<bool> VerifyStoreOwnershipAsync(int storeId, int userId)
        {
            return await _context.Stores
                .AnyAsync(s => s.Id == storeId && s.UserId == userId);
        }
        public async Task<Seller?> GetSellerByIdAsync(int sellerId, CancellationToken cancellationToken = default)
        {
            return await _context.Sellers.FindAsync(new object[] { sellerId }, cancellationToken);
        }

        public async Task<List<Seller>> GetSellersByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Sellers.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<Store> addstores(Store store)
        {
          await _context.Stores.AddAsync(store); 
            await _context.SaveChangesAsync();
            return store;
        }
    }
}
