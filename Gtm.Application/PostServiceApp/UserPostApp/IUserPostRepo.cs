using Gtm.Domain.PostDomain.UserPostAgg;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.UserPostApp
{
    public interface IUserPostRepo : IRepository<UserPost,int>
    {
        Task<UserPost> GetForUserAsync(int userId);
        Task<UserPost> GetByApiCodeAsync(string apiCode);
    }
}
