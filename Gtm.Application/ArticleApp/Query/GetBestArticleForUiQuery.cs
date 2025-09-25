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
    public record GetBestArticleForUiQuery : IRequest<ErrorOr<List<BestArticleQueryModel>>>;
    public class GetBestArtilceForUiQueryHandler : IRequestHandler<GetBestArticleForUiQuery, ErrorOr<List<BestArticleQueryModel>>>
    {
        private readonly  IArticleRepo  _articleRepo;
        private readonly IArticleCategoryRepo  _articleCategoryRepo;

        public GetBestArtilceForUiQueryHandler(IArticleRepo articleRepo, IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepo = articleCategoryRepo;
        }

         

        public async  Task<ErrorOr<List<BestArticleQueryModel>>> Handle(GetBestArticleForUiQuery request, CancellationToken cancellationToken)
        {
            var blogs =   _articleRepo.QueryBy(b => b.IsActive).OrderByDescending(b => b.VisitCount).Take(4);

            var model = await blogs.Select(b => new BestArticleQueryModel
            {
                CategorySlug = "",
                Category = b.CategoryId,
                CategoryTitle = "",
                CreationDate = b.CreateDate.ToPersainDate(),
                Slug = b.Slug,
                SubCategory = b.SubCategoryId,
                Title = b.Title,
                Visit = b.VisitCount,
              
                ImageName = FileDirectories.ArticleImageDirectory + b.ImageName,
                ImageName400 = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                ImageAlt = b.ImageAlt
            }).ToListAsync();

            foreach (var x in model)
            {
                if (x.SubCategory > 0)
                {
                    var sub = await _articleCategoryRepo.GetByIdAsync(x.SubCategory);
                    if (sub is not null)
                    {
                        x.CategorySlug = sub.Slug;
                        x.CategoryTitle = sub.Title;
                    }
                }
                else
                {
                    var parent = await _articleCategoryRepo.GetByIdAsync(x.Category);
                    if (parent is not null)
                    {
                        x.CategorySlug = parent.Slug;
                        x.CategoryTitle = parent.Title;
                    }
                }
            }

            return model;
        }
    }

}
