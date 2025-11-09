using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp
{
    public interface ICommentValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateCommentDto dto);
        Task<ErrorOr<Success>> ValidateGetCommentsForUiAsync(int ownerId, CommentFor commentFor, int pageId);
    }
    public class CommentValidator : ICommentValidator
    {
        private readonly ICommentRepo commentRepo;
        private readonly IUserRepo userRepo;

        public CommentValidator(ICommentRepo commentRepo, IUserRepo userRepo)
        {
            this.commentRepo = commentRepo;
            this.userRepo = userRepo;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateCommentDto dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی کاربر (فقط اگر userId > 0 باشد)
            if (dto.UserId > 0 && !await userRepo.ExistsAsync(u => u.Id ==1))
            {
                errors.Add(Error.Validation("Comment.UserNotFound", "کاربر ارسال کننده یافت نشد."));
            }

            // اعتبارسنجی متن کامنت
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                errors.Add(Error.Validation("Comment.TextRequired", "متن کامنت الزامی است."));
            }
            else if (dto.Text.Length > 1000)
            {
                errors.Add(Error.Validation("Comment.TextTooLong", "متن کامنت نمی‌تواند بیش از 1000 کاراکتر باشد."));
            }

            // اعتبارسنجی ParentId
            //if (dto.ParentId.HasValue && !await commentRepo.ExistsAsync(c => c.Id == dto.ParentId))
            //{
            //    errors.Add(Error.Validation("Comment.ParentNotFound", "کامنت والد یافت نشد."));
            //}

            // اعتبارسنجی ایمیل برای کاربران مهمان (userId == 0)
            if (dto.UserId == 0)
            {
                if (string.IsNullOrEmpty(dto.Email))
                {
                    errors.Add(Error.Validation("Comment.EmailRequired", "برای کاربران مهمان، ایمیل الزامی است."));
                }
                else if (!EmailValidator.IsValid(dto.Email))
                {
                    errors.Add(Error.Validation("Comment.InvalidEmail", "فرمت ایمیل نامعتبر است."));
                }

                if (string.IsNullOrEmpty(dto.FullName))
                {
                    errors.Add(Error.Validation("Comment.FullNameRequired", "برای کاربران مهمان، نام الزامی است."));
                }
                else if (dto.FullName.Length > 100)
                {
                    errors.Add(Error.Validation("Comment.FullNameTooLong", "نام نمی‌تواند بیش از 100 کاراکتر باشد."));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetCommentsForUiAsync(int ownerId, CommentFor commentFor, int pageId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی ownerId
            if (ownerId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Comment.InvalidOwnerId",
                    description: "شناسه مالک نظر نامعتبر است"));
            }

            // اعتبارسنجی commentFor
            if (!Enum.IsDefined(typeof(CommentFor), commentFor))
            {
                errors.Add(Error.Validation(
                    code: "Comment.InvalidCommentFor",
                    description: "نوع نظر نامعتبر است"));
            }

            // اعتبارسنجی pageId
            if (pageId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Comment.InvalidPageId",
                    description: "شماره صفحه نامعتبر است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
