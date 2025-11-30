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
            // 1. دریافت کامنت (با توکن)
            // نکته: اگر آیدی در دامین long است، اینجا هم باید long باشد.
            var comment = await _commentRepository.GetByIdAsync(request.id, cancellationToken);

            // 2. گارد کلاز (بسیار حیاتی)
            if (comment is null)
            {
                return Error.NotFound("Comment.NotFound", "نظر مورد نظر یافت نشد.");
            }

            // 3. اعمال تغییر وضعیت
            comment.Approve();

            // 4. ذخیره
            var saved = await _commentRepository.SaveChangesAsync(cancellationToken);

            if (!saved)
                return Error.Failure("Database.SaveError", "ذخیره سازی وضعیت کامنت با مشکل مواجه شد");

            return Result.Success;
        }
    }
}
