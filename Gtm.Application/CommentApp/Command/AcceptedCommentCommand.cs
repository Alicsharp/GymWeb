using ErrorOr;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.CommentApp.Command
{
    public record AcceptedCommentCommand(int id) : IRequest<ErrorOr<Success>>;
    public class AcceptedCommentCommandHandler : IRequestHandler<AcceptedCommentCommand, ErrorOr<Success>>
    {

        private readonly ICommentRepo _commentRepository;

        public AcceptedCommentCommandHandler(ICommentRepo commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<ErrorOr<Success>> Handle(AcceptedCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.id);
            comment.Approve();
            var saved= await _commentRepository.SaveChangesAsync();
            if (!saved)
                return Error.Failure("NotSaved", "ذخیره سازی به مشکل خورد");

            return Result.Success;
        }
    }
}
