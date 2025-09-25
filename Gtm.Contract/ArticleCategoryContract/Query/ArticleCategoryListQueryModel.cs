using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleCategoryContract.Query
{
    public class ArticleCategoryListQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageName { get; set; }
        public int? ParentId { get; set; }
        public string ParentTitle { get; set; }
        public int ArticlesCount { get; set; }
        public int SubCategoriesCount { get; set; }
    }
}
