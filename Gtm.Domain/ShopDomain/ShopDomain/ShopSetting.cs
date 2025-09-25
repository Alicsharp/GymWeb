using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.ShopDomain
{
    public class ShopSetting : BaseEntity<int>
    {
        public ShopSetting(int sellerDefault)
        {
            SellerDefault = sellerDefault;
        }
        public void Edit(int sellerDefault)
        {
            SellerDefault = sellerDefault;
        }

        public int SellerDefault { get; private set; }
    }
}
