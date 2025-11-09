using ErrorOr;
using Gtm.Application.ShopApp.WishListApp;
using Gtm.Domain.ShopDomain.WishListAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.WishListRepo
{
    internal class WishListRepository : Repository<WishList,int>,IWishListRepository
    {
        private readonly GtmDbContext _context;
        public WishListRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<List<WishList>> GetLast4WishListItemsAsync(int userId)
        {
            return await _context.WishLists
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductSells)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Id) // <-- این بخش برای اطمینان از "آخرین" بودن اضافه شد
                .Take(4)
                .ToListAsync(); // <-- تبدیل به Async
        }
        public async Task<ErrorOr<Success>> DeleteUserProductWishListAsync(int userId, int productId)
        {
            // 1. واکشی به صورت Async
            var wish = await _context.WishLists
                .SingleOrDefaultAsync(s => s.ProductId == productId && s.UserId == userId);

            // 2. بررسی (به جای برگرداندن false)
            if (wish == null)
            {
                return Error.NotFound(description: "آیتم در لیست علاقه‌مندی‌ها یافت نشد.");
            }

            // 3. حذف از حافظه (Context)
            // (استفاده از متد Remove() که در IRepository شما تعریف شده)
            Remove(wish);

            // 4. بازگشت موفقیت (آماده برای ذخیره توسط هندلر)
            return Result.Success;
        }


    }
}
