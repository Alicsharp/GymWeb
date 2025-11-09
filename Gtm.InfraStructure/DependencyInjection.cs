using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.CommentApp;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp;
using Gtm.Application.EmailServiceApp.EmailUserApp;
using Gtm.Application.EmailServiceApp.MessageUserApp;
using Gtm.Application.EmailServiceApp.SensEmailApp;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Application.PostServiceApp.PostPriceApp;
using Gtm.Application.PostServiceApp.PostSettingApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Application.RoleApp;
using Gtm.Application.SeoApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Application.ShopApp.ProductCategoryRelationApp;
using Gtm.Application.ShopApp.ProductFeatureApp;
using Gtm.Application.ShopApp.ProductGalleryApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.ProductVisitApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Application.ShopApp.WishListApp;
using Gtm.Application.SiteServiceApp.BannerApp;
using Gtm.Application.SiteServiceApp.ImageSiteApp;
using Gtm.Application.SiteServiceApp.MenuApp;
using Gtm.Application.SiteServiceApp.SitePageApp;
using Gtm.Application.SiteServiceApp.SiteServiceApp;
using Gtm.Application.SiteServiceApp.SiteSettingApp;
using Gtm.Application.SiteServiceApp.SliderApp;
using Gtm.Application.StoresServiceApp.StoreProductApp;
using Gtm.Application.StoresServiceApp.StroreApp;
using Gtm.Application.TransactionServiceApp;
using Gtm.Application.UserAddressApp;
using Gtm.Application.UserApp;
using Gtm.Application.WalletServiceApp;
using Gtm.InfraStructure.RepoImple.ArticleCategoryRepo;
using Gtm.InfraStructure.RepoImple.ArticleRepo;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Gtm.InfraStructure.RepoImple.EmailRepo;
using Gtm.InfraStructure.RepoImple.OrderDiscountRepo;
using Gtm.InfraStructure.RepoImple.OrderRepo;
using Gtm.InfraStructure.RepoImple.PostServiceRepo;
using Gtm.InfraStructure.RepoImple.ProductCategoryRelationRepo;
using Gtm.InfraStructure.RepoImple.ProductCategoryRepo;
using Gtm.InfraStructure.RepoImple.ProductFeatureRepo;
using Gtm.InfraStructure.RepoImple.ProductGalleryRepo;
using Gtm.InfraStructure.RepoImple.ProductRepo;
using Gtm.InfraStructure.RepoImple.ProductSellRepo;
using Gtm.InfraStructure.RepoImple.ProductVisitRepo;
using Gtm.InfraStructure.RepoImple.RoleRepo;
using Gtm.InfraStructure.RepoImple.SellerRepo;
using Gtm.InfraStructure.RepoImple.SeoRepo;
using Gtm.InfraStructure.RepoImple.SiteRepo;
using Gtm.InfraStructure.RepoImple.StoreRepo;
using Gtm.InfraStructure.RepoImple.TransactionRepo;
using Gtm.InfraStructure.RepoImple.UserRepo;
using Gtm.InfraStructure.RepoImple.WalletRepo;
using Gtm.InfraStructure.RepoImple.WishListRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gtm.InfraStructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfra(this IServiceCollection services,string connection)
        {   
            services.AddScoped<IArticleRepo, ArticleRepository>();
            services.AddScoped<IArticleCategoryRepo, ArticleCategoryRepo>();
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IUserAddressRepo, UserAddressRepository>();
            services.AddScoped<ICommentRepo, CommentRepo>();
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IUserPostRepo, UserPostRepository>();
            services.AddScoped<IPostOrderRepo, PostOrderRepository>();
            services.AddTransient<IStateRepo, StateRepository>();
            services.AddTransient<Application.PostServiceApp.CityApp.ICityRepo, CityRepository>();
            services.AddScoped<Application.PostServiceApp.PostApp.IPostRepo, PostRepository>(); 
            services.AddScoped<IPostPriceRepo, PostPriceRepository>();  
            services.AddScoped<IPackageRepo, PackageRepository>();  
            services.AddScoped<IPostSettingRepo, PostSettingRepository>();
            services.AddScoped<ISeoRepository, SeoRepository>();


            services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
            services.AddScoped<ISiteServiceRepository, SiteServiceRepository>();
            services.AddScoped<IBanerRepository, BanerRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<ISliderRepository, SliderRepository>();
            services.AddScoped<IImageSiteRepository, ImageSiteRepository>();
            services.AddScoped<ISitePageRepository, SitePageRepository>();


            services.AddTransient<IEmailUserRepository, EmailUserRepository>();
            services.AddTransient<ISendEmailRepository, SendEmailRepository>();
            services.AddTransient<IMessageUserRepository, MessageUserRepository>();

            services.AddTransient<ITransactionRepository , TransactionRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            
            services.AddScoped<ISellerRepository,SellerRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRelationRepository, ProductCategoryRelationRepository>();
            services.AddScoped<IProductGalleryRepository, ProductGalleryRepository>();
            services.AddScoped<IProductFeatureRepository,ProductFeatureRepository>();

            services.AddScoped<IStoreProductRepository, StoreProductRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IProductSellRepository, ProductSellRepository>();

            services.AddScoped<IProductDiscountRepository,ProductDiscountRepository>();
            services.AddScoped<IOrderDiscountRepository, OrderDiscountRepository>();
            services.AddScoped<IOrderRepository,OrderRepository>();

            services.AddTransient<IWishListRepository, WishListRepository>();
            services.AddTransient<IProductVisitRepository, ProductVisitRepository>();
            services.AddDbContext<GtmDbContext>(options =>options.UseSqlServer(connection),contextLifetime: ServiceLifetime.Scoped,optionsLifetime: ServiceLifetime.Scoped);
            return services;
        }
    }
}
