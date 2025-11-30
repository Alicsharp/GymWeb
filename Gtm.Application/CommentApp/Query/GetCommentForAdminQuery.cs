using ErrorOr;
using Gtm.Application.ArticleApp;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetCommentForAdminQuery(int pageId, int take, string filter, int ownerId, CommentFor commentFor, CommentStatus status, long? parentId) : IRequest<ErrorOr<CommentAdminPaging>>;
    public class GetCommentForAdminQueryHandler : IRequestHandler<GetCommentForAdminQuery, ErrorOr<CommentAdminPaging>>
    {
        private readonly ICommentRepo _commentRepo;
        private readonly IArticleRepo _articleRepo;
        private readonly IUserRepo _userRepo;

        public GetCommentForAdminQueryHandler(ICommentRepo commentRepo, IArticleRepo articleRepo, IUserRepo userRepo)
        {
            _commentRepo = commentRepo;
            _articleRepo = articleRepo;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<CommentAdminPaging>> Handle(GetCommentForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. ساخت کوئری پایه
            var query = _commentRepo
                .QueryBy(c =>
                    c.CommentFor == request.commentFor &&
                    c.Status == request.status &&
                    (request.ownerId == 0 || c.TargetEntityId == request.ownerId) && // اصلاح شرط OwnerId
                    (request.parentId == null || c.ParentId == request.parentId)     // اصلاح شرط ParentId
                );

            // 2. اعمال فیلتر متنی
            if (!string.IsNullOrWhiteSpace(request.filter))
            {
                query = query.Where(c =>
                    c.FullName.Contains(request.filter) ||
                    c.Email.Contains(request.filter) ||
                    c.Text.Contains(request.filter));
            }

            // اعمال ترتیب
            query = query.OrderByDescending(c => c.Id);

            // 3. تنظیمات صفحه‌بندی
            CommentAdminPaging model = new();
            // فرض: GetData تعداد کل را می‌گیرد و تنظیمات Skip/Take را انجام می‌دهد
            model.GetData(query, request.pageId, request.take, 5);

            // پر کردن متادیتای مدل
            model.Filter = request.filter;
            model.CommentFor = request.commentFor;
            model.CommentStatus = request.status;
            model.OwnerId = request.ownerId;
            model.ParentId = request.parentId;
            model.PageTitle = $"لیست نظرات - {request.status.ToString().Replace("_", " ")}";

            // 4. دریافت داده‌ها از دیتابیس (Async)
            var pagedComments = await query
                .Skip(model.Skip)
                .Take(model.Take)
                .ToListAsync(cancellationToken); // ✅ استفاده از توکن

            // 5. مپ کردن اولیه
            model.Comments = pagedComments.Select(c => new CommentAdminQueryModel
            {
                Id = c.Id,
                CommentFor = c.CommentFor,
                Email = c.Email,
                FullName = c.FullName,
                OwnerId = c.TargetEntityId,
                ParentId = c.ParentId,
                Status = c.Status,
                Text = c.Text,
                UserId = c.AuthorUserId,
                WhyRejected = c.WhyRejected,
                HaveChild = false, // پیش‌فرض
                CommentTitle = "",
                UserName = ""
            }).ToList();

            // 6. پر کردن اطلاعات تکمیلی (Enrichment)

            // الف) تنظیم عنوان کلی صفحه (اگر فیلتر روی مالک خاصی باشد)
            if (model.OwnerId > 0 && request.commentFor == CommentFor.مقاله)
            {
                var blog = await _articleRepo.GetByIdAsync(model.OwnerId, cancellationToken);
                if (blog != null)
                {
                    model.PageTitle += $" - مقاله: {blog.Title}";
                }
            }

            // ب) تنظیم عنوان کلی برای پاسخ‌ها
            if (model.ParentId is > 0)
            {
                var parentComment = await _commentRepo.GetByIdAsync(model.ParentId.Value, cancellationToken);
                if (parentComment != null)
                {
                    model.PageTitle += $" - پاسخ به: {parentComment.FullName}";
                }
            }

            // ج) پر کردن اطلاعات هر ردیف
            foreach (var comment in model.Comments)
            {
                // بررسی فرزند
                comment.HaveChild = await _commentRepo.ExistsAsync(c => c.ParentId == comment.Id, cancellationToken);

                // دریافت نام کاربر
                if (comment.UserId > 0)
                {
                    var user = await _userRepo.GetByIdAsync(comment.UserId, cancellationToken);
                    if (user != null)
                    {
                        comment.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                    }
                }

                // دریافت عنوان آیتم هدف (مقاله/محصول)
                if (comment.CommentFor == CommentFor.مقاله)
                {
                    var blog = await _articleRepo.GetByIdAsync(comment.OwnerId, cancellationToken);
                    if (blog != null)
                    {
                        comment.CommentTitle = blog.Title;
                    }
                }
            }

            return model;
        }
    }

}

