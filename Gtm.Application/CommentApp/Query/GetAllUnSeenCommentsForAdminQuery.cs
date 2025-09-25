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
            var comments = (await _commentRepository.GetAllByQueryAsync(c => c.Status == CommentStatus.خوانده_نشده))
                    .Select(c => new CommentAdminQueryModel
                    {
                        CommentFor = c.CommentFor,
                        Email = c.Email,
                        FullName = c.FullName,
                        HaveChild = false,
                        Id = c.Id,
                        OwnerId = c.TargetEntityId,
                        ParentId = c.ParentId,
                        Status = c.Status,
                        Text = c.Text,
                        UserId = c.AuthorUserId,
                        WhyRejected = c.WhyRejected,
                        CommentTitle = ""
                    }).ToList();

            //});
            foreach (var x in comments)
            {
                x.HaveChild = await _commentRepository.ExistsAsync(c => c.ParentId == x.Id);
                if (x.UserId > 0)
                {
                    try
                    {
                        var user = await _userRepository.GetByIdAsync(x.UserId);
                        x.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                    }
                    catch (Exception ex) { }
                }
                switch (x.CommentFor)
                {
                    case CommentFor.مقاله:
                        var blog = await _blogRepository.GetByIdAsync(x.OwnerId);
                        x.CommentTitle = $"نظر برای مقاله  {blog.Title}";
                        x.CommentTitle = "";
                        break;
                    case CommentFor.محصول:
                        break;
                    case CommentFor.صفحه:
                        var site = await _sitePageRepository.GetByIdAsync(x.OwnerId);
                        x.CommentTitle = $"نظر برای صفحه  {site.Title}";
                        break;
                    default:
                        break;
                }
            }
            return comments;
        }
    }
}
