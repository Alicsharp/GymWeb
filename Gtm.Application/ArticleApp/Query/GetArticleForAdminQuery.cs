using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Query;
 
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetArticleForAdminQuery(int id):IRequest<ErrorOr<AdminArticlesPageQueryModel>>;
    public class GetArticleForAdminQueryHandler : IRequestHandler<GetArticleForAdminQuery, ErrorOr<AdminArticlesPageQueryModel>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleValidator _articleValidator;
        private readonly IArticleCategoryRepo _articleCategoryRepo;

        public GetArticleForAdminQueryHandler(IArticleRepo articleRepo, IArticleValidator articleValidator, IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
            _articleCategoryRepo = articleCategoryRepo;
        }

        public async Task<ErrorOr<AdminArticlesPageQueryModel>> Handle(GetArticleForAdminQuery request, CancellationToken cancellationToken)
        {
            var validatinResults = await _articleValidator.ValidateIdAsync(request.id);
            if(validatinResults.IsError)
            {
              return  validatinResults.Errors;
            }
           
        
               var Articles= await  _articleRepo.GetAllQueryable().AsNoTracking().Select(b=> new ArticleQueryModel
                {
                   Active = b.IsActive,
                   CategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId,
                   CategoryTitle = "",
                   CreationDate = b.CreateDate.ToPersainDate(),
                   Id = b.Id,
                   ImageName = $"{FileDirectories.ArticleImageDirectory100}{b.ImageName}",
                   Title = b.Title,
                   UpdateDate = b.UpdateDate.ToPersainDate(),
                   UserId = b.UserId,
                   VisitCount = b.VisitCount,
                   Writer = b.Writer
               }).ToListAsync();

               string pageTitle;
              if (request.id == 0)
              {
                pageTitle = "لیست تمام مقالات";
               }
             else
              {
                var parentArticle = await _articleCategoryRepo.GetByIdAsync(request.id);
                pageTitle = string.IsNullOrEmpty(parentArticle.Title)
                    ? "لیست مقالات این دسته‌بندی"
                    : $"لیست زیر دسته‌های {parentArticle.Title}";
                }

            return new AdminArticlesPageQueryModel
            {
                CategoryId = request.id,
                PageTitle = pageTitle,
                Article = Articles
            };

 

        }      
         
    }
}
