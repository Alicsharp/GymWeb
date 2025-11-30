using FluentAssertions;
using Gtm.Application.CommentApp;
using Gtm.Application.CommentApp.Query;
using Gtm.Application.UserApp;
using Gtm.Domain.CommentDomain;
using Gtm.Domain.UserDomain.UserDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using System.Reflection;
using Utility.Domain.Enums;

namespace ShopTest.CommandApp.Query
{
    public class GetChildCommentsQueryHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockCommentRepo;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly GetChildCommentsQueryHandler _handler;

        public GetChildCommentsQueryHandlerTests()
        {
            _mockCommentRepo = new Mock<ICommentRepo>();
            _mockUserRepo = new Mock<IUserRepo>();
            _handler = new GetChildCommentsQueryHandler(_mockCommentRepo.Object, _mockUserRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnChildComments_OrderedByDate_WithAvatars()
        {
            // Arrange
            long parentId = 100;
            var queryRequest = new GetChildCommentsQuery(parentId);

            // 1. کامنت‌ها (یکی مال کاربر 1، یکی مال کاربر 2)
            // کامنت قدیمی‌تر (باید اول بیاید)
            var comment1 = CreateComment(1, parentId, userId: 10, text: "First Reply", daysAgo: 2);
            // کامنت جدیدتر (باید دوم بیاید)
            var comment2 = CreateComment(2, parentId, userId: 20, text: "Second Reply", daysAgo: 1);

            var commentsList = new List<Comment> { comment2, comment1 }; // ترتیب در لیست اولیه مهم نیست

            // 2. کاربران
            var user1 = CreateUser(10, "Avatar1.png");
            var user2 = CreateUser(20, "Avatar2.png");
            var usersList = new List<User> { user1, user2 };

            // ماک کردن QueryBy (بازگشت IQueryable)
            var mockDbSet = commentsList.BuildMock();
            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>()))
                .Returns(mockDbSet); // استفاده از .Object برای پکیج

            // ماک کردن GetAllByQueryAsync (بازگشت لیست کاربران)
            _mockUserRepo.Setup(x => x.GetAllByQueryAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(usersList);

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().HaveCount(2);

            // چک کردن ترتیب (بر اساس تاریخ)
            result.Value.First().Text.Should().Be("First Reply");
            result.Value.Last().Text.Should().Be("Second Reply");

            // چک کردن آواتارها
            result.Value.First().Avatar.Should().Contain("Avatar1.png");
            result.Value.Last().Avatar.Should().Contain("Avatar2.png");
        }

        [Fact]
        public async Task Handle_Should_UseDefaultAvatar_When_UserNotFound()
        {
            // Arrange
            long parentId = 100;
            var queryRequest = new GetChildCommentsQuery(parentId);

            // کامنت توسط کاربری نوشته شده که دیگر در سیستم نیست (User ID 99)
            var comment = CreateComment(1, parentId, userId: 99, text: "Ghost User", daysAgo: 1);
            var commentsList = new List<Comment> { comment };

            var mockDbSet = commentsList.BuildMock();
            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>()))
                .Returns(mockDbSet);

            // لیست کاربران خالی برمی‌گردد (کاربر 99 پیدا نشد)
            _mockUserRepo.Setup(x => x.GetAllByQueryAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            // باید آواتار پیش‌فرض استفاده شود
            result.Value.First().Avatar.Should().Contain("default.png");
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_ExceptionOccurs()
        {
            // Arrange
            var queryRequest = new GetChildCommentsQuery(1);

            // شبیه‌سازی خطای دیتابیس
            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>()))
                .Throws(new Exception("DB Connection Lost"));

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Comment.FetchChildError");
            result.FirstError.Description.Should().Contain("DB Connection Lost");
        }

        // --- Helpers ---
        private Comment CreateComment(long id, long parentId, int userId, string text, int daysAgo)
        {
            var comment = new Comment(userId, 1, CommentFor.مقاله, "Name", "Email", text, parentId);
            ForceSetProperty(comment, "Id", id);
            ForceSetProperty(comment, "CreateDate", DateTime.Now.AddDays(-daysAgo));
            ForceSetProperty(comment, "Status", CommentStatus.تایید_شده); // حتما باید تایید شده باشه
            return comment;
        }

        private User CreateUser(int id, string avatar)
        {
            // استفاده از کانستراکتور کامل User که در مراحل قبل دیدیم
            var user = new User("Name", "Mobile", "Email", "Pass", avatar, true, false, Gender.نامشخص);
            ForceSetProperty(user, "Id", id);
            return user;
        }

        private void ForceSetProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite) { prop.SetValue(obj, value); return; }

                var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) { field.SetValue(obj, value); return; }

                type = type.BaseType;
            }
        }
    }
}
