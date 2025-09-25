using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.CommentContract.Query;
using MediatR;
using Utility.Appliation.FileService;
using Utility.Appliation;
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
                var query = _commentRepo
                    .QueryBy(c => c.ParentId == request.ParentId && c.Status == CommentStatus.تایید_شده)
                    .OrderBy(c => c.CreateDate);

                var comments = query.ToList();

                var userIds = comments
                    .Where(c => c.AuthorUserId > 0)
                    .Select(c => c.AuthorUserId)
                    .Distinct()
                    .ToList();

                var users = await _userRepo.GetAllByQueryAsync(u => userIds.Contains(u.Id));
                var userAvatars = users.ToDictionary(
                    u => u.Id,
                    u => FileDirectories.UserImageDirectory100 + u.Avatar
                );

                var result = comments.Select(c => new CommentUiQueryModel
                {
                    Id = c.Id,
                    AuthorUserId = c.AuthorUserId,
                    FullName = c.FullName,
                    CreationDate = c.CreateDate.ToPersainDate(),
                    Text = c.Text,
                    Avatar = userAvatars.ContainsKey(c.AuthorUserId)
                        ? userAvatars[c.AuthorUserId]
                        : "default.png"
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure("Comment.FetchChildError", $"خطا در دریافت پاسخ‌ها: {ex.Message}");
            }
        }
    }

}
