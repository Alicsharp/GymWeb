using Gtm.Domain.ShopDomain.ProductDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.ProductGalleryDomain
{
    public class ProductGallery : BaseEntityCreate<int>
    {
        public int ProductId { get; private set; }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
        public Product Product { get; private set; }
        public ProductGallery()
        {
            Product = new();
        }

        public ProductGallery(int productId, string imageName, string imageAlt)
        {
            ProductId = productId;
            ImageName = imageName;
            ImageAlt = imageAlt;
        }
    }
}
