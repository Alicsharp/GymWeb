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
    public record GetForEditArticleQuery(int Id):IRequest<ErrorOr<UpdateArticleDto>>;
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
            var validationResults= await _articleValidator.ValidateIdAsync(request.Id); 
            if(validationResults.IsError)
            {
                return validationResults.Errors;
            }
            var entity= await _articleRepo.GetByIdAsync(request.Id);
            if(entity == null)
            {
                return Error.NotFound("Article", "مقاله مورد نظر یافت نشد");
            }
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
                Writer= entity.Writer,
            };
        }
    }
}
