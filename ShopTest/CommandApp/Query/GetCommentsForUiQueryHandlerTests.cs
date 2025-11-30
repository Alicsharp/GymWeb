using ErrorOr;
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
    public class GetCommentsForUiQueryHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockCommentRepo;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly Mock<ICommentValidator> _mockValidator;
        private readonly GetCommentsForUiQueryHandler _handler;

        public GetCommentsForUiQueryHandlerTests()
        {
            _mockCommentRepo = new Mock<ICommentRepo>();
            _mockUserRepo = new Mock<IUserRepo>();
            _mockValidator = new Mock<ICommentValidator>();

            _handler = new GetCommentsForUiQueryHandler(
                _mockCommentRepo.Object,
                _mockUserRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnPagedComments_With_CorrectHierarchy_And_Avatars()
        {
            // Arrange
            var query = new GetCommentsForUiQuery(100, CommentFor.مقاله, 1);

            // 1. تنظیم ولیدیشن
            _mockValidator.Setup(x => x.ValidateGetCommentsForUiAsync(100, CommentFor.مقاله, 1))
                .ReturnsAsync(Result.Success);

            // 2. داده‌ها
            // والد 1
            var p1 = CreateComment(1, 100, null, 10, "Parent 1");
            // والد 2
            var p2 = CreateComment(2, 100, null, 20, "Parent 2");
            // فرزند برای والد 1
            var c1 = CreateComment(11, 100, 1, 30, "Child of 1");

            var allComments = new List<Comment> { p1, p2, c1 };

            // کاربرها
            var users = new List<User>
            {
                CreateUser(10, "user1.png"),
                CreateUser(30, "user3.png")
                // کاربر 20 وجود ندارد (باید آواتار پیش‌فرض بگیرد)
            };

            // 3. ستاپ کردن QueryBy (برای گرفتن ریشه‌ها و کانت)
            var mockDbSet = allComments.BuildMock();
            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>()))
                .Returns(mockDbSet);

            // 4. ستاپ کردن GetAllByQueryAsync (برای گرفتن فرزندان)
            // نکته: در کد اصلی ما جداگانه فرزندان را صدا می‌زنیم. اینجا ساده‌سازی می‌کنیم که لیست فرزندان را برگرداند.
            _mockCommentRepo.Setup(x => x.GetAllByQueryAsync(
                    It.Is<Expression<Func<Comment, bool>>>(exp => exp.ToString().Contains("ParentId != null")), // شرط حدودی
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Comment> { c1 });

            // 5. ستاپ کاربران
            _mockUserRepo.Setup(x => x.GetAllByQueryAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // باید 2 والد در صفحه باشند
            result.Value.Comments.Should().HaveCount(2);

            // بررسی والد 1
            var firstParent = result.Value.Comments.First(c => c.Id == 1);
            firstParent.Avatar.Should().Contain("user1.png");
            firstParent.Childs.Should().HaveCount(1);
            firstParent.Childs.First().Text.Should().Be("Child of 1");
            firstParent.Childs.First().Avatar.Should().Contain("user3.png");

            // بررسی والد 2 (که کاربرش پیدا نشد)
            var secondParent = result.Value.Comments.First(c => c.Id == 2);
            secondParent.Avatar.Should().Contain("default.png");
            secondParent.Childs.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_InputInvalid()
        {
            // Arrange
            var query = new GetCommentsForUiQuery(0, CommentFor.مقاله, 1);
            var error = Error.Validation("Id", "Invalid");

            _mockValidator.Setup(x => x.ValidateGetCommentsForUiAsync(0, CommentFor.مقاله, 1))
                .ReturnsAsync(error);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(error);
        }

        // --- Helpers ---
        private Comment CreateComment(long id, int ownerId, long? parentId, int userId, string text)
        {
            var c = new Comment(userId, ownerId, CommentFor.مقاله, "Name", "Mail", text, parentId);
            ForceSetProperty(c, "Id", id);
            ForceSetProperty(c, "Status", CommentStatus.تایید_شده);
            ForceSetProperty(c, "CreateDate", DateTime.Now);
            return c;
        }

        private User CreateUser(int id, string avatar)
        {
            var u = new User("Name", "0912...", "mail", "pass", avatar, true, false, Gender.نامشخص);
            ForceSetProperty(u, "Id", id);
            return u;
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
