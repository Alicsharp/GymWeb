using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Command;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.CommentApp;
using Gtm.Application.EmailServiceApp.EmailUserApp;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Application.PostServiceApp.PostCalculateApp;
using Gtm.Application.PostServiceApp.PostPriceApp;
using Gtm.Application.PostServiceApp.PostSettingApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Application.RoleApp;
using Gtm.Application.SeoApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Application.ShopApp.ProductFeatureApp;
using Gtm.Application.ShopApp.ProductGalleryApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Application.SiteServiceApp.BannerApp;
using Gtm.Application.SiteServiceApp.ImageSiteApp;
using Gtm.Application.SiteServiceApp.MenuApp;
using Gtm.Application.SiteServiceApp.SitePageApp;
using Gtm.Application.SiteServiceApp.SiteServiceApp;
using Gtm.Application.SiteServiceApp.SiteSettingApp;
using Gtm.Application.SiteServiceApp.SliderApp;
using Gtm.Application.TransactionServiceApp;
using Gtm.Application.UserAddressApp;
using Gtm.Application.UserApp;
using Gtm.Application.WalletServiceApp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IArticleValidator, ArticleValidator>();
            services.AddScoped<IArticleCategoryValidator, ArticleCategoryValidator>();
            services.AddScoped<IUserValidator, UserValidator>();
            services.AddScoped<ICommentValidator, CommentValidator>();
            services.AddScoped<IRoleValidatin, RoleValidation>();
            services.AddScoped<ICityValidation, CityValidation>();
            services.AddScoped<IPackageValidation, PackageRepoValidation>();
            services.AddScoped<IPostValidation, PostValidation>();
            services.AddScoped<IUserPostValidation, UserPostValidation>();
            services.AddScoped<IPostPriceValidation, PostPriceValidation>();
            services.AddScoped<IPostSettingValidation, PostSettingValidation>();
            services.AddScoped<IStateValidation, StateValidation>();
            services.AddScoped<IPaymentOrderValidation, PaymentOrderValidationService>();
            services.AddScoped<IPostOrderValidation, PostOrderValidationService>();
            services.AddScoped<IUserAddressValidator, UserAddressValidator>();
            services.AddScoped<ISeoValidator, SeoValidator>();

            services.AddScoped<IBannerValidator, BannerValidator>();
            services.AddScoped<IImageSiteValidator, ImageSiteValidator>();
            services.AddScoped<IMenuValidator, MenuValidator>();
            services.AddScoped<ISitePageValidator, SitePageValidator>();    
            services.AddScoped<ISiteServiceValidator, SiteServiceValidator>();
            services.AddScoped<ISiteSettingValidator, SiteSettingValidator>();
            services.AddScoped<ISliderValidator, SliderValidator>();
            services.AddScoped<ITransactionValidator, TransactionValidator>();
            services.AddScoped<IEmailUserValidator, EmailUserValidator>();
            services.AddScoped<IWalletValidator, WalletValidator>();
            services.AddScoped<ISellerValidator, SellerValidator>();
            services.AddScoped<IProductValidation, ProductValidatior>();
            services.AddScoped<IProductCategoryValidation, ProductCategoryValidationService>();
            services.AddScoped<IProductGalleryValidation, ProductGalleryValidationService>();
            services.AddScoped<IProductFeatureValidation, ProductFeatureValidationService>();
            services.AddScoped<IProductSellValidation, ProductSellValidation>();
            

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining(typeof(CreateArticleCommand));
            });
            return services;
        }
    }
}
