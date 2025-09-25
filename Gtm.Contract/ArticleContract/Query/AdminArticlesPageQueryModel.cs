using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleContract.Query
{
    public class  AdminArticlesPageQueryModel
    {

        public int CategoryId { get; set; }
        public string PageTitle { get; set; }
        public List<ArticleQueryModel> Article { get; set; }
    }
}
