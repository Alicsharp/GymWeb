using Gtm.Application.ArticleApp;
using Gtm.Application.RoleApp;
using Gtm.Application.UserApp;
 
using Gtm.Domain.UserDomain.UserDm;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.UserRepo
{
    public class UserRepo: Repository<User, int>, IUserRepo
    {
        private readonly GtmDbContext  _context;

        public UserRepo(GtmDbContext context):base(context) 
        {
            _context = context;
        }

        public async Task<User?> GetByMobileAsync(string mobile)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Mobile == mobile);
        }
        public async Task<List<User>> GetByIdsAsync(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return new List<User>();

            return await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();
          //  return await _context.Users
          //.Where(u => ids.Contains(u.Id) && !u.IsDelete)
          //  .ToListAsync();
        }
    }
 
}
