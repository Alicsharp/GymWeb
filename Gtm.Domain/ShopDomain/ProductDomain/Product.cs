using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using Gtm.Domain.ShopDomain.ProductFeaureDomain;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using Gtm.Domain.ShopDomain.ProductVisitAgg;
using Gtm.Domain.ShopDomain.WishListAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.ShopDomain.ProductDomain
{

    public class Product : BaseEntityCreateUpdateActive<int>
    {
        public string Title { get; private set; }
        public string Slug { get; private set; }
        public string ShortDescription { get; private set; }
        public string Description { get; private set; }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
        public int Weight { get; private set; }
        public List<ProductCategoryRelation> ProductCategoryRelations { get; private set; }
        public List<ProductFeature> ProductFeatures { get; private set; }
        public List<ProductGallery> ProductGalleries { get; private set; }
        public List<ProductSell> ProductSells { get; private set; }
        public List<ProductVisit> ProductVisits { get; private set; }
        public List<WishList> WishLists { get; private set; }
        public void EditCategoryRelations(List<ProductCategoryRelation> categoryRelations)
        {
            ProductCategoryRelations = categoryRelations;
        }
        public Product()
        {
            ProductCategoryRelations = new();
            ProductFeatures = new();
            ProductGalleries = new();
            ProductSells = new();
            ProductVisits = new();
            WishLists = new();
        }

        public void Edit(string title, string slug, string shortDescription,
            string description, string imageName, string imageAlt, int weight)
        {
            Title = title;
            Slug = slug;
            ShortDescription = shortDescription;
            Description = description;
            ImageName = imageName;
            ImageAlt = imageAlt;
            Weight = weight;
        }
        public Product(string title, string slug, string shortDescription,
            string description, string imageName, string imageAlt, int weight)
        {
            Title = title;
            Slug = slug;
            ShortDescription = shortDescription;
            Description = description;
            ImageName = imageName;
            ImageAlt = imageAlt;
            Weight = weight;
        }
    }
}
