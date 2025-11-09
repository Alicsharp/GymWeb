using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using Gtm.Domain.CommentDomain;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Gtm.Domain.DiscountsDomain.ProductDiscountDomain;
using Gtm.Domain.EmailDomain.EmailUserAgg;
using Gtm.Domain.EmailDomain.MessageUserAgg;
using Gtm.Domain.EmailDomain.SendEmailAgg;
using Gtm.Domain.PostDomain.CityAgg;
using Gtm.Domain.PostDomain.Postgg;
using Gtm.Domain.PostDomain.PostPriceAgg;
using Gtm.Domain.PostDomain.SettingAgg;
using Gtm.Domain.PostDomain.StateAgg;
using Gtm.Domain.PostDomain.UserPostAgg;
using Gtm.Domain.SeoDomain;
using Gtm.Domain.ShopDomain.OrderDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderItemDomain;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using Gtm.Domain.ShopDomain.ProductCategoryDomain;
using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using Gtm.Domain.ShopDomain.ProductDomain;
using Gtm.Domain.ShopDomain.ProductFeaureDomain;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using Gtm.Domain.ShopDomain.ProductVisitAgg;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.ShopDomain.SellerPackageDomain;
using Gtm.Domain.ShopDomain.SellerPackageFeatureDomain;
using Gtm.Domain.ShopDomain.ShopDomain;
using Gtm.Domain.ShopDomain.WishListAgg;
using Gtm.Domain.SiteDomain.BannerAgg;
using Gtm.Domain.SiteDomain.MenuAgg;
using Gtm.Domain.SiteDomain.SitePageAgg;
using Gtm.Domain.SiteDomain.SiteServiceAgg;
using Gtm.Domain.SiteDomain.SiteSettingAgg;
using Gtm.Domain.SiteDomain.SliderAgg;
using Gtm.Domain.StoresDomain.StoreAgg;
using Gtm.Domain.StoresDomain.StoreProductAgg;
using Gtm.Domain.TransactionDomian;
using Gtm.Domain.UserDomain.UserDm;
using Gtm.Domain.UserDomain.WalletAgg;
using Gtm.InfraStructure.EfCodings.ArticelEf;
using Gtm.InfraStructure.EfCodings.ShopEf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.InfraStructure
{
    public class GtmDbContext : DbContext
    {
        public GtmDbContext(DbContextOptions<GtmDbContext> options) : base(options)
        {
        }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleCategory> ArticleCategories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostPrice> PostPrices { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PostOrder> PostOrders { get; set; }
        public DbSet<UserPost> UserPosts { get; set; }
        public DbSet<PostSetting> PostSettings { get; set; }
        public DbSet<Seo> Seos { get; set; }

        public DbSet<Baner> Baners { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<SiteService> SiteServices { get; set; }
        public DbSet<SitePage> SitePages { get; set; }

        public DbSet<EmailUser> EmailUsers { get; set; }
        public DbSet<SendEmail> SendEmails { get; set; }
        public DbSet<MessageUser> MessageUsers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }


        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set; }
        public DbSet<OrderSeller> OrderSellers { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductCategoryRelation> ProductCategoryRelations { get; set; }
        public DbSet<ProductFeature> ProductFeatures { get; set; }
        public DbSet<ProductGallery> ProductGalleries { get; set; }
        public DbSet<ProductSell> ProductSells { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SellerPackage> SellerPackages { get; set; }
        public DbSet<SellerPackageFeature> SellerPackageFeatures { get; set; }
        public DbSet<ShopSetting> ShopSettings { get; set; }

        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreProduct> StoreProducts { get; set; }
        public DbSet<OrderDiscount> OrderDiscounts { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }

        public DbSet<ProductVisit> ProductVisits {  get; set; }
        public DbSet<WishList> WishLists {  get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArticleConfig).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
