using Gtm.Contract.SeoContract.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleContract.Query
{
    public class SingleArticleQueryModel
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategorySlug { get; set; }
        public string CategoryTitle { get; set; }
        public string CreationDate { get; set; }
        public int? ParentId { get; set; }
        public string Writer { get; set; }
        public int VisitCount { get; set; }
        public SeoUiQueryModel Seo { get; set; }
        public List<BreadCrumbQueryModel> BreadCrumb { get; set; }
    }
}
