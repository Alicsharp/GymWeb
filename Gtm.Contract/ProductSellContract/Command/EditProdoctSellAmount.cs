using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.ProductSellContract.Command
{
    public class EditProdoctSellAmount
    {
        public int SellId { get; set; }
        public int count { get; set; }
        public StoreProductType Type { get; set; }
    }
}
