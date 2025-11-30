using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Query;
using Gtm.Domain.BlogDomain.BlogDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace ShopTest.ArticleApp.Query
{
    public class GetBestArticleForSliderUiQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly GetBestArticleForSliderUiQueryHandler _handler;

        public GetBestArticleForSliderUiQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _handler = new GetBestArticleForSliderUiQueryHandler(_mockArticleRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnTop10_SortedByVisitCount()
        {
            // Arrange
            var queryRequest = new GetBestArticleForSliderUiQuery();

            // 1. ساخت 15 مقاله فیک با بازدیدهای مختلف
            var articles = new List<Article>();
            for (int i = 1; i <= 15; i++)
            {
                // بازدیدها را تصادفی یا ترتیبی می‌دهیم.
                // مثلا مقاله 1 بازدیدش 100، مقاله 2 بازدیدش 200 و ...
                articles.Add(CreateArticle(i, $"Art {i}", visitCount: i * 100));
            }

            // نکته مهم در تست QueryBy:
            // چون در کد اصلی QueryBy(x => x.IsActive) صدا زده می‌شود،
            // ما در تست فرض می‌کنیم ریپازیتوری کارش را درست انجام داده و لیست را برگردانده.
            // پس ما لیست مقالات را تبدیل به IQueryable می‌کنیم.

            // استفاده از Helper یا Package
            var mockDbSet = articles.BuildMock();

            // ستاپ کردن متد QueryBy
            // می‌گوییم هر وقت QueryBy با هر شرطی صدا زده شد، این لیست را بده
            _mockArticleRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockDbSet); // اگر هلپر دستی داری .Object نگذار، اگر پکیج داری بگذار

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // 1. باید فقط 10 تا برگرداند (نه 15 تا)
            result.Value.Should().HaveCount(10);

            // 2. باید به ترتیب نزولی بازدید باشد
            // یعنی اولی باید بیشترین بازدید (1500) را داشته باشد
            result.Value.First().VisitCount.Should().Be(1500);
            result.Value.Last().VisitCount.Should().Be(600); // دهمین مقاله

            // 3. چک کنیم که مرتب‌سازی درست است
            result.Value.Should().BeInDescendingOrder(x => x.VisitCount);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmpty_When_NoArticlesExist()
        {
            // Arrange
            var queryRequest = new GetBestArticleForSliderUiQuery();

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

        // --- Helper ---
        private Article CreateArticle(int id, string title, int visitCount)
        {
            var article = new Article(title, "slug", "desc", "text", "img.jpg", "alt", 1, 0, 1, "writer");

            // ست کردن Id و VisitCount با Reflection
            SetProperty(article, "Id", id);
            SetProperty(article, "VisitCount", visitCount);

            return article;
        }

        private void SetProperty(object entity, string propName, object value)
        {
            var prop = entity.GetType().GetProperty(propName);
            if (prop != null)
            {
                if (prop.CanWrite) prop.SetValue(entity, value);
                else prop.SetValue(entity, value, null);
            }
            else
            {
                // تلاش برای فیلد
                var field = entity.GetType().GetField($"<{propName}>k__BackingField",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field?.SetValue(entity, value);
            }
        }
    }
}
