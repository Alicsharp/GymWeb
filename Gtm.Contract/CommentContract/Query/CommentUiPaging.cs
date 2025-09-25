using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.CommentContract.Query
{
    public class CommentUiPaging : BasePaging
    {
        public List<CommentUiQueryModel> Comments { get; set; }
        public int OwnerId { get; set; }
        public CommentFor CommentFor { get; set; }
    }
}
