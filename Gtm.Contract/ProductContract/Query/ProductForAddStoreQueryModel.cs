using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ProductContract.Query
{
    public class ProductForAddStoreQueryModel
    {
        public int ProductId { get; set; }
        public int ProductSellId { get; set; }
        public string ProductTitle { get; set; }
        public string Unit { get; set; }
        public int Price { get; set; }
    }

}
