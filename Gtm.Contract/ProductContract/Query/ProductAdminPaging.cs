using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.ProductContract.Query
{
    public class ProductAdminPaging : BasePaging
    {
        public int CategoryId { get; set; }
        public string Filter { get; set; }
        public string PageTitle { get; set; }
        public ProductAdminOrderBy OrderBy { get; set; }
        public List<ProductQueryAdminModel> Products { get; set; }
    }
}
