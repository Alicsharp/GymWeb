using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleCategoryAdminPageQueryModel
    {
        public int Id { get; set; } 
        public string PageTitle {  get; set; }
        public List<ArticleCategoryAdminQueryModel> articleCategories { get; set; }

    }
}
