using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ProductContract.Command
{
    public class ProductCategoryForAddProduct
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Parent { get; set; }
    }
}
