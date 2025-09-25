using Gtm.Application.CommentApp;
using Gtm.Domain.CommentDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.CommentRepo
{
    public class CommentRepo : Repository<Comment, long>, ICommentRepo
    {
        private readonly GtmDbContext _dbContext;
        public CommentRepo(GtmDbContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<List<Comment>> GetApprovedWithChildsAsync(int ownerId, CommentFor commentFor, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Comments.Include(c=>c.AuthorUserId)
                .Where(c => c.Status == CommentStatus.تایید_شده &&
                            c.AuthorUserId == ownerId &&
                            c.CommentFor == commentFor)
                .ToListAsync(cancellationToken);
        }
        public async Task<List<Comment>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Comments
                .Include(c=>c.Children.Where(c => c.Status == CommentStatus.تایید_شده))
                .Where(c => c.ParentId == parentId && c.Status == CommentStatus.تایید_شده)
                .ToListAsync(cancellationToken);
        }
        public async Task<Dictionary<long, int>> GetGroupedChildCountsAsync(List<long> parentIds)
        {
            return await _dbContext.Comments
                .Where(c => c.ParentId.HasValue && parentIds.Contains(c.ParentId.Value))
                .GroupBy(c => c.ParentId.Value)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ParentId, x => x.Count);
        }

    }
}
