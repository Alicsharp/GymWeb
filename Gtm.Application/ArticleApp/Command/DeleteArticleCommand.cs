using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleApp.Command
{
    public record DeleteArticleCommand(int ArticleId):IRequest<ErrorOr<Success>>;
    public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand, ErrorOr<Success>>
    {
        private readonly IArticleRepo _articleRepo; 
        private readonly IArticleValidator _articleValidator;

        public DeleteArticleCommandHandler(IArticleRepo articleRepo, IArticleValidator articleValidator)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
        {
          var result=    await _articleRepo.RemoveByIdAsync(request.ArticleId);
            if (result)
                return Result.Success;
            return Error.Failure("DeleteArticle", "عملیات حذف شکست خورد");

        }
    }
}
