using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleContract.Query
{
    public class ArticleCardQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
        public int VisitCount { get; set; }
        public string CreateDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
         public string Writer { get; set; }
        public string CategorySlug { get; set; }
        public string? SubCategoryTitle { get; set; }
        public string? SubCategorySlug { get; set; }
    }
}
 
