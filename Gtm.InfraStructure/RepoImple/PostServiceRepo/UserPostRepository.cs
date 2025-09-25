using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Domain.PostDomain.UserPostAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class UserPostRepository : Repository<UserPost,int>, IUserPostRepo
    {
        private readonly GtmDbContext _context;
        public UserPostRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserPost> GetByApiCodeAsync(string apiCode)
        {
            return await _context.UserPosts.SingleOrDefaultAsync(p => p.ApiCode == apiCode);
        }

        public async Task<UserPost> GetForUserAsync(int userId)
        {
            UserPost userPost = await _context.UserPosts.SingleOrDefaultAsync(p => p.UserId == userId);
            if (userPost == null)
            {
                userPost = new UserPost(userId, 50, Guid.NewGuid().ToString());
                await _context.UserPosts.AddAsync(userPost);
              
            }
            return userPost;
        }
    }
}
