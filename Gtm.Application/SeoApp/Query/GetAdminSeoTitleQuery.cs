using ErrorOr;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleCategoryApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.SeoApp.Query
{
    //public record GetAdminSeoTitleQuery(WhereSeo where, int ownerId) : IRequest<ErrorOr<string>>;
    //public class GetAdminSeoTitleQueryHandler : IRequestHandler<GetAdminSeoTitleQuery, ErrorOr<string>>
    //{
    //    private readonly IArticleRepo  _articleRepo;
    //    private readonly IArticleCategoryRepo  _articleCategoryRepo;
    //    private readonly ISitePageRepository _sitePageRepository;
    //    public GetAdminSeoTitleQueryHandler(IBlogRepository blogRepository, IBlogCategoryRepository blogCategoryRepository, ISitePageRepository sitePageRepository)
    //    {
    //        _ArticleRepository = blogRepository;
    //        _ArticleCategoryRepository = blogCategoryRepository;
    //        _sitePageRepository = sitePageRepository;
    //    }
    //    public async Task<ErrorOr<string>> Handle(GetAdminSeoTitleQuery request, CancellationToken cancellationToken)
    //    {
    //        switch (request.where)
    //        {
    //            case WhereSeo.Product:
    //                // GetProductTitle
    //                return "";
    //            case WhereSeo.ProductCategory:
    //                // GetProductCategoryTitle
    //                return "";
    //            case WhereSeo.Blog:
    //                if (request.ownerId < 1) return "";
    //                var blog = await _articleRepo.GetByIdAsync(request.ownerId);
    //                if (blog == null) return "";
    //                return $"seo برای مقاله {blog.Title}";
    //            case WhereSeo.BlogCategory:
    //                if (request.ownerId < 1) return "seo برای صفحه اصلی مقالات";
    //                var blogCategory = await _articleCategoryRepo.GetByIdAsync(request.ownerId);
    //                if (blogCategory == null) return "";
    //                return $"seo برای دسته بندی مقاله {blogCategory.Title}";
    //            case WhereSeo.Home:
    //                return "seo برای صفحه اصلی سایت";
    //            case WhereSeo.About:
    //                return "seo برای صفحه درباره ما";
    //            case WhereSeo.Contact:
    //                return "seo برای صفحه تماس با ما";
    //            case WhereSeo.Page:
    //                if (request.ownerId < 1) return "";
    //                var page = await _sitePageRepository.GetByIdAsync(request.ownerId);
    //                if (page == null) return "";
    //                return $"seo برای صفحه {page.Title}";
    //            case WhereSeo.PostPackage:
    //                return "seo برای صفحه پکیج های فروش Api پست";
    //            default:
    //                return "";
    //        }
    //    }
    //}
    public record GetAdminSeoTitleQuery(WhereSeo where, int ownerId) : IRequest<ErrorOr<string>>;
    public class GetAdminSeoTitleQueryHandler : IRequestHandler<GetAdminSeoTitleQuery, ErrorOr<string>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetAdminSeoTitleQueryHandler(IArticleRepo articleRepo, IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<string>> Handle(GetAdminSeoTitleQuery request, CancellationToken cancellationToken)
        {
            switch (request.where)
            {
                case WhereSeo.Product:
                    // GetProductTitle
                    return "";
                case WhereSeo.ProductCategory:
                    // GetProductCategoryTitle
                    return "";
                case WhereSeo.Blog:
                    if (request.ownerId < 1) return "";
                    var blog = await _articleRepo.GetByIdAsync(request.ownerId);
                    if (blog == null) return "";
                    return $"seo برای مقاله {blog.Title}";
                case WhereSeo.BlogCategory:
                    if (request.ownerId < 1) return "seo برای صفحه اصلی مقالات";
                    var blogCategory = await _articleCategoryRepo.GetByIdAsync(request.ownerId);
                    if (blogCategory == null) return "";
                    return $"seo برای دسته بندی مقاله {blogCategory.Title}";
                case WhereSeo.Home:
                    return "seo برای صفحه اصلی سایت";
                case WhereSeo.About:
                    return "seo برای صفحه درباره ما";
                case WhereSeo.Contact:
                    return "seo برای صفحه تماس با ما";
               
                case WhereSeo.PostPackage:
                    return "seo برای صفحه پکیج های فروش Api پست";
                default:
                    return "";
            }
        }
    }
}
