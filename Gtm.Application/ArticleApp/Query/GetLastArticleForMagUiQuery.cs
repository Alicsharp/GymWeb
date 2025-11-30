using ErrorOr;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetLastArticleForMagUiQuery : IRequest<ErrorOr<List<LastArticleForMagQueryModel>>>;

    public class GetLastBlogForMagUiQueryHandler : IRequestHandler<GetLastArticleForMagUiQuery, ErrorOr<List<LastArticleForMagQueryModel>>>
    {
        private readonly IArticleRepo _articleRepo;

        public GetLastBlogForMagUiQueryHandler(IArticleRepo articleRepo)
        {
            _articleRepo = articleRepo;
        }

        public async Task<ErrorOr<List<LastArticleForMagQueryModel>>> Handle(GetLastArticleForMagUiQuery request, CancellationToken cancellationToken)
        {
            var blogs = await _articleRepo
               .QueryBy(b => b.IsActive)
               .OrderByDescending(b => b.CreateDate) // جدیدترین‌ها اول
               .Take(5) // فقط ۵ تا
               .Select(b => new LastArticleForMagQueryModel(b.Slug, b.Title))
               .ToListAsync(cancellationToken); // ✅ پاس دادن توکن

            return blogs;
        }
    }
}
