using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.CommentApp;
using Gtm.Application.CommentApp.Query;
using Gtm.Application.SiteServiceApp.SitePageApp;
using Gtm.Application.UserApp;
using Gtm.Domain.BlogDomain.BlogDm;
using Gtm.Domain.CommentDomain;
using Gtm.Domain.UserDomain.UserDm;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace ShopTest.CommandApp.Query
{
    public class GetAllUnSeenCommentsForAdminQueryHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockCommentRepo;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly Mock<ISitePageRepository> _mockSitePageRepo;
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly GetAllUnSeenCommentsForAdminQueryHnadler _handler;

        public GetAllUnSeenCommentsForAdminQueryHandlerTests()
        {
            _mockCommentRepo = new Mock<ICommentRepo>();
            _mockUserRepo = new Mock<IUserRepo>();
            _mockSitePageRepo = new Mock<ISitePageRepository>();
            _mockArticleRepo = new Mock<IArticleRepo>();

            _handler = new GetAllUnSeenCommentsForAdminQueryHnadler(
                _mockCommentRepo.Object,
                _mockUserRepo.Object,
                _mockSitePageRepo.Object,
                _mockArticleRepo.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnArticleTitle_When_CommentIsForArticle()
        {
            // Arrange
            var query = new GetAllUnSeenCommentsForAdminQuery();

            // 1. کامنت برای مقاله
            var comments = new List<Comment>
            {
                CreateComment(id: 1, ownerId: 100, type: CommentFor.مقاله, userId: 0, text: "Good")
            };

            // 2. مقاله مربوطه
            var article = new Article("Tech News", "slug", "desc", "text", "img", "alt", 1, 0, 1, "writer");
            ForceSetProperty(article, "Id", 100);

            // ستاپ‌ها
            _mockCommentRepo.Setup(x => x.GetAllByQueryAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments);

            _mockArticleRepo.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(article);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            var item = result.Value.First();

            item.CommentFor.Should().Be(CommentFor.مقاله);
            item.CommentTitle.Should().Contain("Tech News"); // بررسی باگ تایتل خالی
        }

        [Fact]
        public async Task Handle_Should_ReturnUserName_When_UserIdIsPositive()
        {
            // Arrange
            var query = new GetAllUnSeenCommentsForAdminQuery();
            int userId = 50;

            var comments = new List<Comment>
    {
        CreateComment(id: 1, ownerId: 10, type: CommentFor.محصول, userId: userId, text: "Hi")
    };

            // ✅ اصلاح شده:
            // 1. ترتیب پارامترها طبق کلاس User اصلاح شد (موبایل، ایمیل، پسورد، آواتار...)
            // 2. پارامتر Gender به آخر اضافه شد
            var user = new User(
                "Reza",             // FullName
                "09120000000",      // Mobile
                "reza@mail.com",    // Email
                "pass",             // Password
                "avatar.png",       // Avatar
                true,               // IsActive
                false,              // IsDelete
                Gender.مرد          // Gender (اینجا قبلاً جا افتاده بود)
            );

            ForceSetProperty(user, "Id", userId);

            _mockCommentRepo.Setup(x => x.GetAllByQueryAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments);

            _mockUserRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.First().UserName.Should().Be("Reza");
        }

        [Fact]
        public async Task Handle_Should_SetHaveChildTrue_When_CommentHasReply()
        {
            // Arrange
            var query = new GetAllUnSeenCommentsForAdminQuery();
            int commentId = 1;

            var comments = new List<Comment>
            {
                CreateComment(id: commentId, ownerId: 10, type: CommentFor.محصول, userId: 0, text: "Parent")
            };

            _mockCommentRepo.Setup(x => x.GetAllByQueryAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(comments);

            // فرض می‌کنیم در دیتابیس فرزندی برای این کامنت وجود دارد (Exists = true)
            _mockCommentRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.First().HaveChild.Should().BeTrue();
        }

        // --- Helper Methods ---
        private Comment CreateComment(long id, int ownerId, CommentFor type, int userId, string text)
        {
            var comment = new Comment(userId, ownerId, type, "Name", "Email", text, null);
            ForceSetProperty(comment, "Id", id);

            // وضعیت باید خوانده نشده باشد (برای ستاپ اولیه تست)
            // اما چون GetAllByQueryAsync را ماک می‌کنیم، مقدار Status دقیق مهم نیست مگر اینکه داخل هندلر چک شود

            return comment;
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
