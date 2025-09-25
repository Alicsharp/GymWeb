using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record GetArticleCategoriesForAddArticleQuery : IRequest<ErrorOr<List<ArticleCategoryForAddArticleQueryModel>>>;

    public class GetArticleCategoriesForAddArticleQueryHandler: IRequestHandler<GetArticleCategoriesForAddArticleQuery, ErrorOr<List<ArticleCategoryForAddArticleQueryModel>>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetArticleCategoriesForAddArticleQueryHandler(IArticleCategoryRepo articleCategoryRepo)
        {
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<List<ArticleCategoryForAddArticleQueryModel>>> Handle(GetArticleCategoriesForAddArticleQuery request,CancellationToken cancellationToken)
        {
            var categories = await _articleCategoryRepo.GetAllQueryable().Include(c => c.Children)
                .Where(c => c.ParentId == null) // فقط دسته‌بندی‌های اصلی
                .Select(c => new ArticleCategoryForAddArticleQueryModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    SubCategories = c.Children.Select(sc => new ArticleCategoryForAddArticleQueryModel
                    {
                        Id = sc.Id,
                        Title = sc.Title
                    }).ToList()
                })
                .ToListAsync(cancellationToken);


            return categories;
        }
    }
}
