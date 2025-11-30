using ErrorOr;
using Gtm.Contract.ArticleContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetForEditArticleQuery(int Id) : IRequest<ErrorOr<UpdateArticleDto>>;
    public class GetForEditArticleQueryHandler : IRequestHandler<GetForEditArticleQuery, ErrorOr<UpdateArticleDto>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleValidator _articleValidator;

        public GetForEditArticleQueryHandler(IArticleRepo articleRepo, IArticleValidator articleValidator)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
        }

        public async Task<ErrorOr<UpdateArticleDto>> Handle(GetForEditArticleQuery request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی آیدی
            var validationResults = await _articleValidator.ValidateIdAsync(request.Id);
            if (validationResults.IsError)
            {
                return validationResults.Errors;
            }

            // 2. دریافت از دیتابیس (با توکن)
            var entity = await _articleRepo.GetByIdAsync(request.Id, cancellationToken); // <--- توکن اضافه شد

            // 3. گارد کلاز (اگر پیدا نشد)
            if (entity == null)
            {
                return Error.NotFound("Article.NotFound", "مقاله مورد نظر یافت نشد");
            }

            // 4. مپ کردن به DTO
            return new UpdateArticleDto
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                SubCategoryId = entity.SubCategoryId,
                Text = entity.Text,
                ImageAlt = entity.ImageAlt,
                ImageName = entity.ImageName,
                ShortDescription = entity.ShortDescription,
                Slug = entity.Slug,
                Title = entity.Title,
                Writer = entity.Writer,
            };
        }
    }
}