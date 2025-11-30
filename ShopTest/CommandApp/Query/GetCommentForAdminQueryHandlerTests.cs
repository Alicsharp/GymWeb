using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.CommentApp;
using Gtm.Application.CommentApp.Query;
using Gtm.Application.UserApp;
using Gtm.Domain.BlogDomain.BlogDm;
using Gtm.Domain.CommentDomain;
using Gtm.Domain.UserDomain.UserDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using System.Reflection;
using Utility.Domain.Enums;

namespace ShopTest.CommandApp.Query
{
    public class GetCommentForAdminQueryHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockCommentRepo;
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly GetCommentForAdminQueryHandler _handler;

        public GetCommentForAdminQueryHandlerTests()
        {
            _mockCommentRepo = new Mock<ICommentRepo>();
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockUserRepo = new Mock<IUserRepo>();

            _handler = new GetCommentForAdminQueryHandler(
                _mockCommentRepo.Object,
                _mockArticleRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnFilteredComments_And_EnrichData()
        {
            // Arrange
            var query = new GetCommentForAdminQuery(
                pageId: 1,
                take: 10,
                filter: "",
                ownerId: 0,
                commentFor: CommentFor.مقاله,
                status: CommentStatus.خوانده_نشده,
                parentId: null
            );

            // 1. ساخت کامنت
            var comment = new Comment(100, 50, CommentFor.مقاله, "Ali", "mail", "Nice Post", null);
            SetId(comment, 1);
            // وضعیت و نوع باید با کوئری مچ باشد
            ForceSetProperty(comment, "Status", CommentStatus.خوانده_نشده);

            var commentsList = new List<Comment> { comment };
            var mockDbSet = commentsList.BuildMock();

            // 2. ساخت یوزر
            var user = new User("Reza User", "0912...", "mail", "pass", "img", true, false, Gender.مرد);

            // 3. ساخت مقاله
            var article = new Article("Article Title", "slug", "desc", "text", "img", "alt", 1, 0, 1, "writer");

            // Setup Repo Queries
            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>()))
                .Returns(mockDbSet);

            // Setup Enrichment lookups
            _mockUserRepo.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(50, It.IsAny<CancellationToken>())).ReturnsAsync(article);
            _mockCommentRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Comments.Should().HaveCount(1);

            var resultItem = result.Value.Comments.First();

            // چک کردن پر شدن نام کاربر
            resultItem.UserName.Should().Be("Reza User");

            // چک کردن پر شدن تایتل مقاله
            resultItem.CommentTitle.Should().Be("Article Title");
        }

        [Fact]
        public async Task Handle_Should_NotCrash_When_RelatedEntityIsNull()
        {
            // Arrange
            var query = new GetCommentForAdminQuery(1, 10, "", 0, CommentFor.مقاله, CommentStatus.خوانده_نشده, null);

            // کامنت وجود دارد ولی کاربر و مقاله حذف شده‌اند
            var comment = new Comment(999, 888, CommentFor.مقاله, "Guest", "mail", "Text", null);
            SetId(comment, 1);
            ForceSetProperty(comment, "Status", CommentStatus.خوانده_نشده);

            var mockDbSet = new List<Comment> { comment }.BuildMock();

            _mockCommentRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Comment, bool>>>())).Returns(mockDbSet);

            // بازگشت Null برای وابستگی‌ها
            _mockUserRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(888, It.IsAny<CancellationToken>())).ReturnsAsync((Article?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            var item = result.Value.Comments.First();

            // باید مقادیر پیش‌فرض باشند و ارور ندهد
            item.UserName.Should().BeNullOrEmpty();
            item.CommentTitle.Should().BeNullOrEmpty();
        }

        // --- Helpers ---
        private void SetId(object entity, int id) => ForceSetProperty(entity, "Id", id);

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
