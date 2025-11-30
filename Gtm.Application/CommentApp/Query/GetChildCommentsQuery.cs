using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetChildCommentsQuery(long ParentId) : IRequest<ErrorOr<List<CommentUiQueryModel>>>;
    public class GetChildCommentsQueryHandler : IRequestHandler<GetChildCommentsQuery, ErrorOr<List<CommentUiQueryModel>>>
    {
        private readonly ICommentRepo _commentRepo;
        private readonly IUserRepo _userRepo;

        public GetChildCommentsQueryHandler(ICommentRepo commentRepo, IUserRepo userRepo)
        {
            _commentRepo = commentRepo;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<List<CommentUiQueryModel>>> Handle(GetChildCommentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. دریافت کامنت‌های فرزند (Async + Token)
                // نکته: ToListAsync کوئری را اجرا می‌کند و دیتا را به رم می‌آورد
                var comments = await _commentRepo
                    .QueryBy(c => c.ParentId == request.ParentId && c.Status == CommentStatus.تایید_شده)
                    .OrderBy(c => c.CreateDate)
                    .ToListAsync(cancellationToken);

                if (!comments.Any())
                    return new List<CommentUiQueryModel>();

                // 2. استخراج آیدی کاربرها
                var userIds = comments
                    .Where(c => c.AuthorUserId > 0)
                    .Select(c => c.AuthorUserId)
                    .Distinct()
                    .ToList();

                // 3. دریافت اطلاعات کاربران به صورت Bulk (یک کوئری)
                var users = await _userRepo.GetAllByQueryAsync(u => userIds.Contains(u.Id), cancellationToken);

                // تبدیل به دیکشنری برای جستجوی سریع O(1)
                var userAvatars = users.ToDictionary(
                    u => u.Id,
                    u => FileDirectories.UserImageDirectory100 + u.Avatar
                );

                // 4. مپ کردن نهایی (در حافظه)
                var result = comments.Select(c => new CommentUiQueryModel
                {
                    Id = c.Id,
                    AuthorUserId = c.AuthorUserId,
                    FullName = c.FullName,
                    CreationDate = c.CreateDate.ToPersianDate(), // تبدیل تاریخ در حافظه امن است
                    Text = c.Text,

                    // اگر آواتار کاربر پیدا شد که هیچ، وگرنه مسیر پیش‌فرض
                    Avatar = userAvatars.ContainsKey(c.AuthorUserId)
                        ? userAvatars[c.AuthorUserId]
                        : FileDirectories.UserImageDirectory100 + "default.png"
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا در اینجا پیشنهاد می‌شود
                return Error.Failure("Comment.FetchChildError", $"خطا در دریافت پاسخ‌ها: {ex.Message}");
            }
        }
    }

}
