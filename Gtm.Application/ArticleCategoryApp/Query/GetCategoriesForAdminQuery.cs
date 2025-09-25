using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record GetCategoriesForAdminQuery(int id):IRequest<ErrorOr<ArticleCategoryAdminPageQueryModel>>;
    public class GetCategoriesForAdminQueryHandler : IRequestHandler<GetCategoriesForAdminQuery, ErrorOr<ArticleCategoryAdminPageQueryModel>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo; 
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public GetCategoriesForAdminQueryHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<ArticleCategoryAdminPageQueryModel>> Handle(GetCategoriesForAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _articleCategoryValidator.ValidateIdAsync(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
             
            if (request.id > 0)
            {
                var subCategories = await _articleCategoryRepo.QueryBy(c => c.ParentId == request.id)
                    .AsNoTracking()
                    .Select(c => new ArticleCategoryAdminQueryModel
                    {
                        Active = c.IsActive,
                        CreationDate = c.CreateDate.ToPersainDate(),
                        Id = c.Id,
                        
                        ImageName = $"/Images/ArticleCategory/100/{c.ImageName}", 
                        Title = c.Title ?? string.Empty,
                        UpdateDate = c.UpdateDate.ToPersainDate()
                    })
                    .ToListAsync();

                var parentCategory = await _articleCategoryRepo.GetByIdAsync(request.id);
                if (parentCategory is null)
                    return Error.NotFound("Category.Parent.NotFound", "دسته‌بندی والد یافت نشد.");

                return new ArticleCategoryAdminPageQueryModel
                {
                    Id = request.id,
                    articleCategories = subCategories,
                    PageTitle = $"لیست زیر دسته‌های {parentCategory.Title}"
                };
            }
            else
            {
                var categories = await _articleCategoryRepo.QueryBy(c => c.ParentId == null)
                    .AsNoTracking()
                    .Select(c => new ArticleCategoryAdminQueryModel
                    {
                        Active = c.IsActive,
                        CreationDate = c.CreateDate.ToPersainDate(),
                        Id = c.Id,
                        //ImageName = c.ImageName,
                        ImageName = $"/Images/ArticleCategory/100/{c.ImageName}",
                        Title = c.Title ?? string.Empty,
                        UpdateDate = c.UpdateDate.ToPersainDate()
                    })
                    .ToListAsync();

                return new ArticleCategoryAdminPageQueryModel
                {
                    Id = 0,
                    articleCategories = categories,
                    PageTitle = "لیست سر دسته‌های مقاله"
                };

            }


        }
    }
}
