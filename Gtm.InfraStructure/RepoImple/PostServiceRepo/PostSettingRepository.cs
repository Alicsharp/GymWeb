using Gtm.Application.PostServiceApp.PostSettingApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.UserApp;
using Gtm.Contract.PostContract.PostSettingContract.Command;
using Gtm.Contract.PostContract.StateContract.Command;
using Gtm.Contract.PostContract.StateContract.Query;
using Gtm.Domain.PostDomain.SettingAgg;
using Gtm.Domain.PostDomain.StateAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class PostSettingRepository : Repository<PostSetting,int>, IPostSettingRepo
    {
        private readonly GtmDbContext _context;
        public PostSettingRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UbsertPostSetting> GetForUbsertAsync()
        {
            var s = await GetSingleAsync();
            return new()
            {
                PackageDescription = s.PackageDescription,
                PackageTitle = s.PackageTitle
            };
        }

        public async Task<PostSetting> GetSingleAsync()
        {
            var setting = _context.PostSettings.SingleOrDefault();
            if (setting == null)
            {
                setting = new("", "", "");
                await AddAsync(setting);
            }
            return setting;
        }
    }
   
}
