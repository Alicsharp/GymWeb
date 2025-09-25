using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.ProductCategoryContract.Query;
using Gtm.Contract.SeoContract.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.ProductContract.Query
{
    public class ShopPaging : BasePaging
    {
        public int ShopId { get; set; }
        public string ShopTitle { get; set; }
        public string CategorySlug { get; set; }
        public string Filter { get; set; }
        public ShopOrderBy OrderBy { get; set; }
        public List<ProductShopUiQueryModel> Products { get; set; }
        public List<ProductCategoryUiQueryModel> Categories { get; set; }
        public List<BreadCrumbQueryModel> BreadCrumb { get; set; }
        public SeoUiQueryModel Seo { get; set; }
    }
}
   
