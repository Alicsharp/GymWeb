using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record ArticleCategoryActiveChangeCommand(int Id):IRequest<ErrorOr<Success>>;
    public class ArticleCategoryActiveChangeCommandHandler : IRequestHandler<ArticleCategoryActiveChangeCommand, ErrorOr<Success>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _articleCategoryValidator;

        public ArticleCategoryActiveChangeCommandHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator articleCategoryValidator)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _articleCategoryValidator = articleCategoryValidator;
        }

        public async Task<ErrorOr<Success>> Handle(ArticleCategoryActiveChangeCommand request, CancellationToken cancellationToken)
        {
            var validationResults= await _articleCategoryValidator.ValidateIdAsync(request.Id); 
            if(validationResults.IsError)
            {
                return validationResults.Errors;
            }
            var entity=await _articleCategoryRepo.GetByIdAsync(request.Id);
            if(entity != null)
            {
                entity.ActivationChange();
                var result= await _articleCategoryRepo.SaveChangesAsync(cancellationToken);
                if(result ==true)  
                return Result.Success;
                return Error.Failure("ActiveChange", "عملیات فعال یا غیر فعال سازی شکست خورد");

            }
            return Error.Failure("ActiveChange", "عملیات فعال یا غیر فعال سازی شکست خورد");
        }
    }
}
