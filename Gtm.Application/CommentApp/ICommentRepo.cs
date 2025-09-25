using Gtm.Domain.CommentDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp
{
    public interface ICommentRepo:IRepository<Comment,long>
    {
        Task<List<Comment>> GetApprovedWithChildsAsync(int ownerId, CommentFor commentFor, CancellationToken cancellationToken = default);
        Task<List<Comment>> GetChildrenAsync(long parentId, CancellationToken cancellationToken = default);
        Task<Dictionary<long, int>> GetGroupedChildCountsAsync(List<long> parentIds);


    }
}
