using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Query;
using Gtm.Domain.BlogDomain.BlogDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopTest.ArticleApp.Query
{
    public class GetLastArticleForMagUiQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly GetLastBlogForMagUiQueryHandler _handler;

        public GetLastArticleForMagUiQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _handler = new GetLastBlogForMagUiQueryHandler(_mockArticleRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnTop5ActiveArticles_OrderedByDateDesc()
        {
            // Arrange
            var queryRequest = new GetLastArticleForMagUiQuery();

            var articles = new List<Article>
            {
                CreateArticle(1, "Newest", isActive: true, daysAgo: 0),
                CreateArticle(2, "Yesterday", isActive: true, daysAgo: 1),
                CreateArticle(3, "Old 1", isActive: true, daysAgo: 2),
                CreateArticle(4, "Old 2", isActive: true, daysAgo: 3),
                CreateArticle(5, "Old 3", isActive: true, daysAgo: 4),
                CreateArticle(6, "Too Old", isActive: true, daysAgo: 10),
                CreateArticle(7, "Inactive New", isActive: false, daysAgo: 0) // این باید حذف شود
            };

            // ✅ اصلاح کلیدی:
            // به جای اینکه لیست ثابت را برگردانیم، شرط (predicate) را می‌گیریم و اعمال می‌کنیم
            _mockArticleRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns((Expression<Func<Article, bool>> predicate) =>
                {
                    // 1. شرط را روی لیست اعمال می‌کنیم
                    var filtered = articles.AsQueryable().Where(predicate);

                    // 2. نتیجه فیلتر شده را ماک می‌کنیم
                    return filtered.BuildMock();
                });

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().HaveCount(5);

            // حالا این تست باید پاس شود چون Inactive New توسط کد بالا حذف شده
            result.Value.Should().NotContain(x => x.Title == "Inactive New");
            result.Value.Should().NotContain(x => x.Title == "Too Old");
            result.Value.First().Title.Should().Be("Newest");
            result.Value.Last().Title.Should().Be("Old 3");
        }

        [Fact]
        public async Task Handle_Should_ReturnEmpty_When_NoActiveArticlesExist()
        {
            // Arrange
            var queryRequest = new GetLastArticleForMagUiQuery();

            // لیست خالی
            var emptyList = new List<Article>().BuildMock();

            _mockArticleRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(emptyList);

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().BeEmpty();
        }

        // --- Helper Methods ---

        private Article CreateArticle(int id, string title, bool isActive, int daysAgo)
        {
            var article = new Article(title, "slug", "desc", "text", "img", "alt", 1, 0, 1, "writer");

            ForceSetProperty(article, "Id", id);
            ForceSetProperty(article, "IsActive", isActive);

            // تنظیم تاریخ ساخت (برای تست OrderBy)
            ForceSetProperty(article, "CreateDate", DateTime.Now.AddDays(-daysAgo));

            return article;
        }

        // همان متد قدرتمند برای ست کردن مقادیر در کلاس پدر
        private void ForceSetProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value);
                    return;
                }

                var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }
        }
    }
}
