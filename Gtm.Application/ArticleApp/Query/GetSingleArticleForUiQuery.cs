using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetSingleArticleForUiQuery(string slug) : IRequest<ErrorOr<SingleArticleQueryModel>>;
    public class GetSingleArticleForUiQueryHandler : IRequestHandler<GetSingleArticleForUiQuery, ErrorOr<SingleArticleQueryModel>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetSingleArticleForUiQueryHandler(IArticleRepo articleRepo,IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<SingleArticleQueryModel>> Handle(GetSingleArticleForUiQuery request, CancellationToken cancellationToken)
        {
            var article = await _articleRepo.GetBySlugAsync(request.slug);
            if (article == null)
                return Error.NotFound(description: "مقاله مورد نظر یافت نشد");

            var categoryId = article.SubCategoryId ==  null ? article.CategoryId : article.SubCategoryId;
            var category = await _articleCategoryRepo.GetByIdAsync(categoryId);

            var model = new SingleArticleQueryModel
            {
           
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Description = article.Text,
                ImageName = $"/Images/Article/400/{article.ImageName}",
                ImageAlt = article.ImageAlt,
                CreationDate = article.CreateDate.ToPersainDate(),
                VisitCount = article.VisitCount,
                CategoryId = categoryId,
                CategoryTitle = category?.Title ?? "بدون دسته‌بندی",
                CategorySlug = category?.Slug ?? "",
                BreadCrumb = await GetArticleBreadCrumb(article, category)
            };

            return model;
        }

        private async Task<List<BreadCrumbQueryModel>> GetArticleBreadCrumb(Article article, ArticleCategory mainCategory)
        {
            var breadcrumbs = new List<BreadCrumbQueryModel>
        {
            new() { Title = "خانه", Url = "/" },
            new() { Title = "مقالات", Url = "/articles" },
            new() { Title = "آرشیو مقالات", Url = "/articles/archive" }
        };

            if (mainCategory != null)
            {
                breadcrumbs.Add(new()
                {
                    Title = mainCategory.Title,
                    Url = $"/articles/category/{mainCategory.Slug}"
                });

                if (article.SubCategoryId > 0 && article.SubCategoryId != article.CategoryId)
                {
                    var subCategory = await _articleCategoryRepo.GetByIdAsync(article.SubCategoryId);
                    if (subCategory != null)
                    {
                        breadcrumbs.Add(new()
                        {
                            Title = subCategory.Title,
                            Url = $"/articles/category/{subCategory.Slug}"
                        });
                    }
                }
            }

            breadcrumbs.Add(new() { Title = article.Title, Url = "" });
            return breadcrumbs;
        }
    }
}
