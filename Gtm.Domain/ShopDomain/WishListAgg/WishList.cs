using Gtm.Domain.ShopDomain.ProductDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.WishListAgg
{
    public class WishList : BaseEntity<int>
    {
        public int ProductId { get; private set; }
        public int UserId { get; private set; }
        public Product Product { get; private set; }
        public WishList()
        {
            Product = new Product();
        }

        public WishList(int productId, int userId)
        {
            ProductId = productId;
            UserId = userId;
        }
    }

}
