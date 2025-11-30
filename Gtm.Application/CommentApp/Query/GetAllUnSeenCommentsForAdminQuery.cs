using ErrorOr;
using Gtm.Application.ArticleApp;
using Gtm.Application.SiteServiceApp.SitePageApp;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.CommentApp.Query
{
    public record GetAllUnSeenCommentsForAdminQuery : IRequest<ErrorOr<List<CommentAdminQueryModel>>>;
    public class GetAllUnSeenCommentsForAdminQueryHnadler : IRequestHandler<GetAllUnSeenCommentsForAdminQuery, ErrorOr<List<CommentAdminQueryModel>>>
    {
        private readonly ICommentRepo _commentRepository;
        private readonly IUserRepo _userRepository;
        private readonly ISitePageRepository _sitePageRepository;
        private readonly IArticleRepo _blogRepository;

        public GetAllUnSeenCommentsForAdminQueryHnadler(ICommentRepo commentRepository, IUserRepo userRepository, ISitePageRepository sitePageRepository, IArticleRepo blogRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _sitePageRepository = sitePageRepository;
            _blogRepository = blogRepository;
        }

        public async Task<ErrorOr<List<CommentAdminQueryModel>>> Handle(GetAllUnSeenCommentsForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت کامنت‌های خوانده نشده
            var commentsRaw = await _commentRepository.GetAllByQueryAsync(c => c.Status == CommentStatus.خوانده_نشده, cancellationToken);

            // تبدیل اولیه (Projection)
            var resultList = commentsRaw.Select(c => new CommentAdminQueryModel
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
                HaveChild = false, // مقدار پیش‌فرض
                CommentTitle = "", // مقدار پیش‌فرض
                UserName = ""      // مقدار پیش‌فرض
            }).ToList();

            // 2. پر کردن اطلاعات تکمیلی (حلقه)
            foreach (var item in resultList)
            {
                // الف) بررسی وجود پاسخ (فرزند)
                item.HaveChild = await _commentRepository.ExistsAsync(c => c.ParentId == item.Id, cancellationToken);

                // ب) دریافت نام کاربر (اگر عضو سایت باشد)
                if (item.UserId > 0)
                {
                    var user = await _userRepository.GetByIdAsync(item.UserId, cancellationToken);
                    if (user != null)
                    {
                        item.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                    }
                }

                // ج) دریافت عنوان مقاله یا صفحه
                switch (item.CommentFor)
                {
                    case CommentFor.مقاله:
                        var blog = await _blogRepository.GetByIdAsync(item.OwnerId, cancellationToken);
                        if (blog != null)
                        {
                            item.CommentTitle = $"نظر برای مقاله: {blog.Title}";
                        }
                        // خط باگ‌دار حذف شد ✅
                        break;

                    case CommentFor.محصول:
                        // فعلاً خالی
                        item.CommentTitle = "نظر برای محصول";
                        break;

                    case CommentFor.صفحه:
                        var site = await _sitePageRepository.GetByIdAsync(item.OwnerId, cancellationToken);
                        if (site != null)
                        {
                            item.CommentTitle = $"نظر برای صفحه: {site.Title}";
                        }
                        break;
                }
            }

            return resultList;
        }
    }
}
