using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Command;
using Gtm.Domain.PostDomain.UserPostAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class PackageRepository : Repository<Package, int>, IPackageRepo
    {
        private readonly GtmDbContext _context;
        public PackageRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CreatePostOrder> GetCreatePostModelAsync(int userId, int packageId)
        {
            var package = await _context.Packages.FindAsync(packageId);
            return new CreatePostOrder(userId, package.Id, package.Price);
        }

        public async Task<EditPackage> GetForEditAsync(int id)
        {
            return await _context.Packages.Select(p => new EditPackage
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Count = p.Count,
                Price = p.Price,
                ImageAlt = p.ImageAlt,
                ImageFile = null,
                ImageName = p.ImageName
            }).SingleOrDefaultAsync(p => p.Id == id);
        }
    }
}
