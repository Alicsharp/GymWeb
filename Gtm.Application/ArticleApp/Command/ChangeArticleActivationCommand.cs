using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleApp.Command
{
    public record ChangeArticleActivationCommand(int Id):IRequest<ErrorOr<Success>>;
    public class ChangeArticleActivationCommandHandler : IRequestHandler<ChangeArticleActivationCommand, ErrorOr<Success>>
    { 
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleValidator _articleValidator;

        public ChangeArticleActivationCommandHandler(IArticleRepo articleRepo, IArticleValidator articleValidator)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
        }

        public async Task<ErrorOr<Success>> Handle(ChangeArticleActivationCommand request, CancellationToken cancellationToken)
        {
             var validationResults= await _articleValidator.ValidateIdAsync(request.Id);
             if(validationResults.IsError)
             {
                 return validationResults.Errors;
             }
            var entity=await _articleRepo.GetByIdAsync(request.Id);
            entity.ActivationChange();
           var saved= await _articleRepo.SaveChangesAsync(cancellationToken);
            if(!saved)
            {
                return Error.Failure("NotSavaed", "عملیات شکست خورد");
            }
            return Result.Success;
        }
    }
}
