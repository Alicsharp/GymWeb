using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetCommentsForUiQuery(int TargetEntityId, CommentFor commentFor, int pageId ):IRequest<ErrorOr<CommentUiPaging>>;
    public class GetCommentsForUiQueryHandler : IRequestHandler<GetCommentsForUiQuery, ErrorOr<CommentUiPaging>>
    {
        private readonly ICommentRepo _commentRepository;
        private readonly IUserRepo _userRepository;
        private readonly ICommentValidator _validator;

        public GetCommentsForUiQueryHandler(ICommentRepo commentRepository,IUserRepo userRepository,ICommentValidator validator)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<CommentUiPaging>> Handle(GetCommentsForUiQuery request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی
            var validationResult = await _validator.ValidateGetCommentsForUiAsync(
                request.TargetEntityId, request.commentFor, request.pageId);

            if (validationResult.IsError) return validationResult.Errors;

            try
            {
                var model = new CommentUiPaging();

                // 2. کوئری پایه (فقط برای شمارش و فیلتر اولیه)
                var baseQuery = _commentRepository.QueryBy(c =>
                    c.Status == CommentStatus.تایید_شده &&
                    c.TargetEntityId == request.TargetEntityId &&
                    c.CommentFor == request.commentFor &&
                    c.ParentId == null); // فقط ریشه‌ها برای صفحه‌بندی

                // محاسبه تعداد کل و تنظیمات پیجینگ (Count Query)
                model.GetData(baseQuery, request.pageId, 3, 2); // متد GetData باید روی IQueryable کانت بگیرد

                // 3. دریافت نظرات والد (Paged Root Comments)
                var rootComments = await baseQuery
                    .OrderByDescending(c => c.CreateDate)
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .ToListAsync(cancellationToken);

                if (!rootComments.Any())
                {
                    model.Comments = new List<CommentUiQueryModel>();
                    return model;
                }

                // 4. دریافت پاسخ‌ها (فقط پاسخ‌های مربوط به کامنت‌های این صفحه)
                var rootIds = rootComments.Select(c => c.Id).ToList();
                var childComments = await _commentRepository.GetAllByQueryAsync(
                    c => c.ParentId != null && rootIds.Contains(c.ParentId.Value) && c.Status == CommentStatus.تایید_شده,
                    cancellationToken);

                // 5. دریافت اطلاعات کاربران (Bulk Load - حل مشکل N+1)
                var allUserIds = rootComments.Select(c => c.AuthorUserId)
                    .Concat(childComments.Select(c => c.AuthorUserId))
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                var users = await _userRepository.GetAllByQueryAsync(u => allUserIds.Contains(u.Id), cancellationToken);

                // ساخت دیکشنری برای دسترسی سریع O(1)
                var userAvatars = users.ToDictionary(
                    u => u.Id,
                    u => FileDirectories.UserImageDirectory100 + (u.Avatar ?? "default.png"));

                // 6. مپ کردن نهایی در حافظه
                model.OwnerId = request.TargetEntityId;
                model.CommentFor = request.commentFor;

                model.Comments = rootComments.Select(parent => new CommentUiQueryModel
                {
                    Id = parent.Id,
                    AuthorUserId = parent.AuthorUserId,
                    FullName = parent.FullName,
                    Text = parent.Text,
                    CreationDate = parent.CreateDate.ToPersianDate(),
                    Avatar = GetAvatar(parent.AuthorUserId, userAvatars),

                    // وصل کردن فرزندان مربوطه
                    Childs = childComments
                        .Where(child => child.ParentId == parent.Id)
                        .OrderBy(child => child.CreateDate)
                        .Select(child => new CommentUiQueryModel
                        {
                            Id = child.Id,
                            AuthorUserId = child.AuthorUserId,
                            FullName = child.FullName,
                            Text = child.Text,
                            CreationDate = child.CreateDate.ToPersianDate(),
                            Avatar = GetAvatar(child.AuthorUserId, userAvatars),
                            Childs = new List<CommentUiQueryModel>()
                        }).ToList()
                }).ToList();

                return model;
            }
            catch (Exception ex)
            {
                return Error.Failure("Comment.FetchError", $"خطا در دریافت نظرات: {ex.Message}");
            }
        }

        // متد کمکی برای گرفتن آواتار
        private string GetAvatar(int userId, Dictionary<int, string> avatars)
        {
            if (userId > 0 && avatars.ContainsKey(userId))
                return avatars[userId];
            return FileDirectories.UserImageDirectory100 + "default.png";
        }
    }

}
