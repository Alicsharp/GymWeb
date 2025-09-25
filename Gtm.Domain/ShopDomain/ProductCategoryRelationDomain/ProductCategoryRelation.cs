using Gtm.Domain.ShopDomain.ProductCategoryDomain;
using Gtm.Domain.ShopDomain.ProductDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.ProductCategoryRelationDomain
{
    public class ProductCategoryRelation : BaseEntity<int>
    {
        public int ProductId { get; internal set; }
        public int ProductCategoryId { get; private set; }
        public Product Product { get; private set; }
        public ProductCategory ProductCategory { get; private set; }
        public ProductCategoryRelation()
        {
            Product = new();
            ProductCategory = new();
        }
        public ProductCategoryRelation(int productCategoryId)
        {
            ProductCategoryId = productCategoryId;
        }
        public ProductCategoryRelation(int productId, int productCategoryId)
        {
            ProductId = productId;
            ProductCategoryId = productCategoryId;
        }
    }

}
