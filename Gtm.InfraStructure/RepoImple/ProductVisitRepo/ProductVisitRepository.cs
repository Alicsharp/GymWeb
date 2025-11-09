using Gtm.Application.ShopApp.ProductVisitApp;
using Gtm.Domain.ShopDomain.ProductVisitAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductVisitRepo
{
    internal class ProductVisitRepository : Repository<ProductVisit, int>, IProductVisitRepository
    {
        private readonly GtmDbContext _context;
        public ProductVisitRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> UbsertProductVisitAsymc(ProductVisit command)
        {
            if (await ExistsAsync(c => c.ProductId == command.ProductId && c.UserId == command.UserId))
            {
                ProductVisit visit = await _context.ProductVisits.SingleAsync
                    (c => c.ProductId == command.ProductId && c.UserId == command.UserId);
                visit.AddVisit();
                return await SaveChangesAsync();
            }
            else
            {
                // حالت افزودن (با استفاده از IRepository)
                await AddAsync(command); // 1. افزودن به حافظه
                return await SaveChangesAsync(); // 2. ذخیره در دیتابیس
            }
        }
        public async Task<int> GetTotalVisitCountAsync(CancellationToken cancellationToken = default)
        {
            // 2. از متد Query() (که در IRepository شما بود) استفاده می‌کنیم
            // 3. و SumAsync را روی آن اجرا می‌کنیم (بر اساس کد قبلی شما، فیلد 'Count' جمع زده می‌شود)
            return await Query().SumAsync(p => p.Count, cancellationToken);
        }
    }
}