using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class PostOrderRepository : Repository<PostOrder,int>, IPostOrderRepo
    {
        private readonly GtmDbContext _context;
        public PostOrderRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PostOrderUserPanelModel> GetPostOrderNotPaymentForUser(int userId)
        {
            var postOrder = await GetPostOrderNotPaymentForUserAsync(userId);
            if (postOrder == null) { return null; }
            var package = await _context.Packages.FindAsync(postOrder.PackageId);
            if (package.Price != postOrder.Price)
            {
                postOrder.Edit(package.Id, package.Price);
                await _context.SaveChangesAsync();
            }
            return new PostOrderUserPanelModel(postOrder.Id, postOrder.PackageId, package.Title, postOrder.Price,
             $"{FileDirectories.PackageImageDirectory400}{package.ImageName}", package.ImageAlt, package.Count, package.Description);
        }

        public async Task<PostOrder> GetPostOrderNotPaymentForUserAsync(int userId) =>
            await _context.PostOrders.SingleOrDefaultAsync(p => p.UserId == userId && p.Status == PostOrderStatus.پرداخت_نشده);
    }
}
