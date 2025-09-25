using Gtm.Contract.SeoContract.Query;
using Utility.Appliation;

namespace Gtm.Contract.ArticleContract.Query
{
    public class ArticleUiPaging : BasePaging
    {
        public List<ArticleCardQueryModel> Articles { get; set; }
        public List<BreadCrumbQueryModel> BreadCrumb { get; set; }
        public List<ArticleCategorySearchQueryModel> Categories { get; set; }
        public SeoUiQueryModel Seo { get; set; }
        public string Filter { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
    }
}
 
