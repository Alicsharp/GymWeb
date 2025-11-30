using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Query;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopTest.ArticleApp.Query
{
    public class GetBestArtilceForUiQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleCategoryRepo> _mockCategoryRepo;
        private readonly GetBestArtilceForUiQueryHandler _handler;

        public GetBestArtilceForUiQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockCategoryRepo = new Mock<IArticleCategoryRepo>();
            _handler = new GetBestArtilceForUiQueryHandler(_mockArticleRepo.Object, _mockCategoryRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnTop4_SortedByVisit_WithCorrectCategoryInfo()
        {
            // Arrange
            var queryRequest = new GetBestArticleForUiQuery();

            // 1. ساخت دسته‌بندی‌ها
            var catSport = new ArticleCategory("Sport", "sport", "img", "alt");
            SetId(catSport, 10);

            var catFootball = new ArticleCategory("Football", "football", "img", "alt", 10);
            SetId(catFootball, 20);

            _mockCategoryRepo.Setup(x => x.GetAllByQueryAsync(
                    It.IsAny<Expression<Func<ArticleCategory, bool>>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new List<ArticleCategory> { catSport, catFootball });

            // 2. ساخت مقالات
            var articles = new List<Article>
            {
                CreateArticle(1, "Top 1", visit: 500, catId: 10, subCatId: 20),
                CreateArticle(2, "Top 2", visit: 400, catId: 10, subCatId: 0),
                CreateArticle(3, "Top 3", visit: 300, catId: 10, subCatId: 0),
                CreateArticle(4, "Top 4", visit: 200, catId: 10, subCatId: 0),
                CreateArticle(5, "Low Visit", visit: 50, catId: 10, subCatId: 0)
            };

            // تبدیل لیست با پکیج
            var mockDbSet = articles.BuildMock();

            // استفاده از .Object برای پکیج MockQueryable
            _mockArticleRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockDbSet);

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().HaveCount(4);

            // چک کردن ترتیب و مقادیر
            result.Value.First().Visit.Should().Be(500);

            // چک کردن مپینگ دسته‌بندی
            result.Value.First().CategoryTitle.Should().Be("Football"); // چون SubCategoryId 20 بود
            result.Value.Skip(1).First().CategoryTitle.Should().Be("Sport"); // چون فقط CategoryId 10 داشت
        }

        [Fact]
        public async Task Handle_Should_ReturnEmpty_When_NoArticlesExist()
        {
            // Arrange
            var queryRequest = new GetBestArticleForUiQuery();

            var emptyList = new List<Article>().BuildMock();

            _mockArticleRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(emptyList);

            // Act
            var result = await _handler.Handle(queryRequest, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().BeEmpty();
        }

        // --- Helper Methods (بخش اصلاح شده) ---

        private Article CreateArticle(int id, string title, int visit, int catId, int subCatId)
        {
            var article = new Article(title, "slug", "desc", "text", "img.jpg", "alt", catId, subCatId, 1, "writer");

            // استفاده از متد قدرتمند ForceSetProperty
            SetId(article, id);
            ForceSetProperty(article, "VisitCount", visit);
            ForceSetProperty(article, "CreateDate", DateTime.Now);
            ForceSetProperty(article, "IsActive", true);

            return article;
        }

        private void SetId(object entity, int id) => ForceSetProperty(entity, "Id", id);

        // ✅ این متد اصلاح شده است تا کلاس‌های پدر را هم بگردد (حل مشکل ارور شما)
        private void ForceSetProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();

            // حلقه می‌زنیم تا به بالاترین سطح وراثت برسیم (مثلا تا BaseEntity)
            while (type != null)
            {
                // 1. تلاش برای پیدا کردن پراپرتی
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value);
                    return;
                }

                // 2. تلاش برای پیدا کردن فیلد پشتیبان (Backing Field)
                var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }

                // اگر در این کلاس نبود، برو یک پله بالاتر (کلاس پدر)
                type = type.BaseType;
            }
        }
    }
}
