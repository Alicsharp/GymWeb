using ErrorOr;
using Gtm.Contract.CommentContract.Command;
using Gtm.Domain.CommentDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.CommentApp.Command
{
    public record CreateCommentCommand(CreateCommentDto Command):IRequest<ErrorOr<Success>>;
    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ErrorOr<Success>>
    {
        private readonly  ICommentRepo _commentRepo;
        private readonly ICommentValidator _commentValidator;

        public CreateCommentCommandHandler(ICommentRepo commentRepo, ICommentValidator commentValidator)
        {
            _commentRepo = commentRepo;
            _commentValidator = commentValidator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی ورودی‌ها
            var validationResults = await _commentValidator.ValidateCreateAsync(request.Command);
            if (validationResults.IsError)
            {
                return validationResults.Errors;
            }

            // 2. بررسی وجود کامنت والد (اگر پاسخ به نظر دیگری است) 
            if (request.Command.ParentId is > 0)
            {
                // فرض بر این است که متد GetByIdAsync در ریپازیتوری کامنت وجود دارد
                // اگر کامنت پدر پیدا نشد، نباید اجازه ثبت داد
                var parentComment = await _commentRepo.GetByIdAsync(request.Command.ParentId.Value, cancellationToken);
                if (parentComment is null)
                {
                    return Error.NotFound("Comment.ParentNotFound", "نظری که می‌خواهید به آن پاسخ دهید یافت نشد.");
                }
            }

            // 3. ساخت موجودیت
            Comment newComment = new Comment(
                authorUserId: request.Command.UserId,
                targetEntityId: request.Command.OwnerId,
                commentFor: request.Command.CommentFor,
                fullName: request.Command.FullName,
                email: request.Command.Email,
                text: request.Command.Text,
                parentId: request.Command.ParentId);

            // 4. افزودن و ذخیره
            await _commentRepo.AddAsync(newComment);

            var isSaved = await _commentRepo.SaveChangesAsync(cancellationToken);
            if (!isSaved)
            {
                return Error.Failure("Comment.SaveFailed", "ثبت نظر با خطا مواجه شد.");
            }

            return Result.Success;
        }
    }
}
