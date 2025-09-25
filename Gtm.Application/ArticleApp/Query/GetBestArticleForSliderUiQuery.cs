using ErrorOr;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetBestArticleForSliderUiQuery : IRequest<ErrorOr<List<BestArticleSliderQueryModel>>>;
    public class GetBestArticleForSliderUiQueryHandler : IRequestHandler<GetBestArticleForSliderUiQuery, ErrorOr<List<BestArticleSliderQueryModel>>>
    { 
        private readonly IArticleRepo _articleRepo;

        public GetBestArticleForSliderUiQueryHandler(IArticleRepo articleRepo)
        {
            _articleRepo = articleRepo;
        }

        public async Task<ErrorOr<List<BestArticleSliderQueryModel>>> Handle(GetBestArticleForSliderUiQuery request, CancellationToken cancellationToken)
        {
            var query = _articleRepo.QueryBy(b => b.IsActive);

            var result = await query
                .OrderByDescending(b => b.VisitCount)
                .Take(10)
                .Select(b => new BestArticleSliderQueryModel
                {
                    ImageAlt = b.ImageAlt,
                    Id = b.Id,
                    ImageName = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                    Slug = b.Slug,
                    Title = b.Title,
                    VisitCount = b.VisitCount
                }).ToListAsync();

            return result;
        }
    }
}
