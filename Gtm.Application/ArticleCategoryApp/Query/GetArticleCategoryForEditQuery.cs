using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Query
{
    public record GetArticleCategoryForEditQuery(int Id):IRequest<ErrorOr<UpdateArticleCategoryDto>>;
    public class GetArticleCategoryForEditQueryHandler : IRequestHandler<GetArticleCategoryForEditQuery, ErrorOr<UpdateArticleCategoryDto>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public GetArticleCategoryForEditQueryHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<UpdateArticleCategoryDto>> Handle(GetArticleCategoryForEditQuery request, CancellationToken cancellationToken)
        {
            var validationResult= await _articleCategoryValidator.ValidateIdAsync(request.Id);
            if(validationResult.IsError) 
            {
                return validationResult.Errors;
            }
            var entity = await _articleCategoryRepo.GetByIdAsync(request.Id);
            if(entity == null) 
            {
                return Error.Failure("ArticleCategoryNotFound", "کتگوری مورد نظر یافت نشد");
            }
            return new UpdateArticleCategoryDto()
            {
                ImageAlt = entity.ImageAlt,
                Id = entity.Id,
                ImageFile = null,
                ImageName = entity.ImageName,
                Parent = entity.ParentId.Value,
                Slug = entity.Slug,
                Title = entity.Title
            };
        }
    }
}
