using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.SeoApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.SeoContract.Query;
using Gtm.Domain.BlogDomain.BlogDm;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;


namespace Gtm.Application.ArticleApp.Query
{
    public record GetArticlesForUiQuery(string slug, int pageId, string filter) : IRequest<ErrorOr<ArticleUiPaging>>;
    public class GetBlogsForUiQueryHandler : IRequestHandler<GetArticlesForUiQuery, ErrorOr<ArticleUiPaging>>
    {
        private readonly IArticleRepo _ArticleRepository;
        private readonly IArticleCategoryRepo _ArticleCategoryRepository;
        private readonly ISeoRepository _seoRepository;
        private readonly IArticleValidator _validator;

        public GetBlogsForUiQueryHandler(IArticleRepo blogRepository,IArticleCategoryRepo blogCategoryRepository,ISeoRepository seoRepository,IArticleValidator validator)
        {
            _ArticleRepository = blogRepository;
            _ArticleCategoryRepository = blogCategoryRepository;
            _seoRepository = seoRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<ArticleUiPaging>> Handle(GetArticlesForUiQuery request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetBlogsForUiAsync(
                request.slug,
                request.pageId,
                request.filter);

            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                string slug = request.slug;
                string title = "آرشیو مقالات";
                int ownerSeoId = 0;
                string categoryTitle = "آرشیو مقالات";

                // تغییر این بخش برای رفع خطا
                var query = _ArticleRepository.GetAllQueryable().Where(b => b.IsActive);
                IOrderedQueryable<Article> res = query.OrderByDescending(b => b.Id);

                if (!string.IsNullOrEmpty(request.slug))
                {
                    var category = await _ArticleCategoryRepository.GetBySlugAsync(request.slug);
                    if (category == null) slug = "";
                    res = query.Where(r => r.CategoryId == category.Id || r.SubCategoryId == category.Id)
                              .OrderByDescending(b => b.Id);
                    title = title + " " + category.Title;
                    ownerSeoId = category.Id;
                    categoryTitle = category.Title;
                }

                if (!string.IsNullOrEmpty(request.filter))
                {
                    res = query.Where(b => b.Title.Contains(request.filter) ||
                                         b.ShortDescription.Contains(request.filter) ||
                                         b.Text.Contains(request.filter))
                              .OrderByDescending(b => b.Id);
                    title = title + " شامل : " + request.filter;
                }

                ArticleUiPaging model = new();
                model.GetData(res, request.pageId, 4, 2);
                model.Filter = request.filter;
                model.Slug = request.slug;
                model.Title = title;
                model.Articles = new List<ArticleCardQueryModel>();

                if (await res.AnyAsync(cancellationToken))
                {
                    model.Articles = await res.Skip(model.Skip)
                                              .Take(model.Take)
                                              .Select(b => new ArticleCardQueryModel
                                              {
                                                  ImageAlt = b.ImageAlt,
                                                  CategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId,
                                                  CategoryTitle = "",
                                                  CategorySlug = "",
                                                  CreateDate = b.CreateDate.ToPersainDate(),
                                                  Id = b.Id,
                                                  ImageName = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                                                  ShortDescription = b.ShortDescription,
                                                  Slug = b.Slug,
                                                  Title = b.Title,
                                              })
                                              .OrderByDescending(b => b.Id)
                                              .ToListAsync(cancellationToken);

                    foreach (var article in model.Articles)
                    {
                        var category = await _ArticleCategoryRepository.GetByIdAsync(article.CategoryId);
                        article.CategoryTitle = category?.Title ?? "";
                        article.CategorySlug = category?.Slug ?? "";
                    }
                }

                var seo = await _seoRepository.GetSeoForUiAsync(ownerSeoId, WhereSeo.BlogCategory, categoryTitle);
                model.Seo = new SeoUiQueryModel(seo.MetaTitle, seo.MetaDescription, seo.MetaKeyWords, seo.IndexPage, seo.Canonical, seo.Schema);
                model.Categories = await GetArchiveBlogCategories();
                model.BreadCrumb = await GetArchiveBreadCrumb(request.slug);

                return model;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Article.FetchError",
                    description: $"خطا در دریافت مقالات: {ex.Message}");
            }
          

            
        }
        private async Task<List<BreadCrumbQueryModel>> GetArchiveBreadCrumb(string slug)
        {
            List<BreadCrumbQueryModel> model = new()
        {
            new BreadCrumbQueryModel(){Number = 1 , Title ="خانه" , Url = "/"} ,
            new BreadCrumbQueryModel(){Number = 2, Title = "مجله خبری" , Url = "/Blog"},
            new BreadCrumbQueryModel(){Number = 3, Title = "آرشیو مقالات" , Url = "/Blogs"}
        };
            if (!string.IsNullOrEmpty(slug))
            {
                var category = await _ArticleCategoryRepository.GetBySlugAsync(slug);
                if (category != null)
                    model.Add(new BreadCrumbQueryModel() { Number = 4, Title = category.Title, Url = "" });
            }
            return model;
        }
        private async Task<List<ArticleCategorySearchQueryModel>> GetArchiveBlogCategories()
        {
            var parents = await _ArticleCategoryRepository.GetAllByQueryAsync(b => b.IsActive && b.ParentId == 0);

            var model = new List<ArticleCategorySearchQueryModel>();

            foreach (var parent in parents)
            {
                var parentBlogCount = (await _ArticleRepository
                    .GetAllByQueryAsync(b => b.IsActive && (b.CategoryId == parent.Id || b.SubCategoryId == parent.Id)))
                    .Count();

                var childEntities = await _ArticleCategoryRepository.GetAllByQueryAsync(b => b.IsActive && b.ParentId == parent.Id);

                var childModels = new List<ArticleCategorySearchQueryModel>();

                foreach (var child in childEntities)
                {
                    var childBlogCount = (await _ArticleRepository
                        .GetAllByQueryAsync(c => c.IsActive && (c.CategoryId == child.Id || c.SubCategoryId == child.Id)))
                        .Count();

                    childModels.Add(new ArticleCategorySearchQueryModel
                    {
                        Id = child.Id,
                        Title = child.Title,
                        Slug = child.Slug,
                        ArticleCount = childBlogCount,
                        Childs = new()
                    });
                }

                model.Add(new ArticleCategorySearchQueryModel
                {
                    Id = parent.Id,
                    Title = parent.Title,
                    Slug = parent.Slug,
                    ArticleCount = parentBlogCount,
                    Childs = childModels
                });
            }

            return model;
        }
        // بقیه متدها بدون تغییر...
    }

}
