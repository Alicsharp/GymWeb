using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.CommentContract.Query
{
    public class CommentAdminPaging : BasePaging
    {
        public List<CommentAdminQueryModel> Comments { get; set; }
        public string Filter { get; set; }
        public CommentFor CommentFor { get; set; }
        public CommentStatus CommentStatus { get; set; }
        public int OwnerId { get; set; }
        public long? ParentId { get; set; }
        public string PageTitle { get; set; }
    }
}
