using Gtm.Domain.ShopDomain.ProductDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.ProductFeaureDomain
{
    public class ProductFeature : BaseEntity<int>
    {
        public int ProductId { get; private set; }
        public string Title { get; private set; }
        public string Value { get; private set; }
        public Product Product { get; private set; }
        public ProductFeature()
        {
            Product = new();
        }

        public ProductFeature(int productId, string title, string value)
        {
            ProductId = productId;
            Title = title;
            Value = value;
        }
    }
}
