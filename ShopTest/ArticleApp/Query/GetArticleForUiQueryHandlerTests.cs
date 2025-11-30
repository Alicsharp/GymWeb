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
    public class GetArticleForUiQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleCategoryRepo> _mockCategoryRepo;
        private readonly GetArticleForUiQueryHandler _handler;

        public GetArticleForUiQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockCategoryRepo = new Mock<IArticleCategoryRepo>();
            _handler = new GetArticleForUiQueryHandler(_mockArticleRepo.Object, _mockCategoryRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnAllActiveArticles_When_NoFilterProvided()
        {
            // Arrange
            var query = new GetArticleForUiQuery(slug: "", pageId: 1, filter: "");

            // 1. ساخت دیتای مقالات با هلپر جدید
            var articles = new List<Article>
            {
                CreateArticle(1, "Active Art 1", isActive: true),
                CreateArticle(2, "Active Art 2", isActive: true),
                CreateArticle(3, "Inactive Art", isActive: false)
            };

            // تبدیل لیست به دیتابیس مجازی (با استفاده از پکیج)
            var mockDbSet = articles.BuildMock();

            // ستاپ کردن ریپازیتوری (حتما با .Object)
            _mockArticleRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet);

            // ستاپ کردن سایدبار (برای جلوگیری از نال شدن)
            _mockCategoryRepo.Setup(x => x.GetAllByQueryAsync(
                    It.IsAny<Expression<Func<ArticleCategory, bool>>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new List<ArticleCategory>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // باید فقط 2 مقاله فعال را برگرداند
            result.Value.Articles.Should().HaveCount(2);

            // نباید مقاله غیرفعال را داشته باشد
            result.Value.Articles.Should().NotContain(a => a.Title == "Inactive Art");

            result.Value.Title.Should().Be("آرشیو مقالات");
        }

        // --- Helper Methods (بخش جادویی) ---

        private Article CreateArticle(int id, string title, bool isActive)
        {
            // استفاده از کانستراکتور خود کلاس برای مقادیر عادی
            var article = new Article(title, "slug", "desc", "text", "img.jpg", "alt", 1, 0, 1, "writer");

            // استفاده از رفلکشن هوشمند برای مقادیر کلاس پدر (BaseEntity)
            SetPrivateProperty(article, "Id", id);
            SetPrivateProperty(article, "IsActive", isActive);
            SetPrivateProperty(article, "CreateDate", DateTime.Now); // مهم برای جلوگیری از خطای ToPersianDate

            return article;
        }

        // ✅ این متد اصلاح شده است تا کلاس‌های پدر را هم بگردد
        private void SetPrivateProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();

            // حلقه می‌زنیم تا به بالاترین سطح وراثت برسیم (مثلا تا BaseEntity)
            while (type != null)
            {
                // 1. تلاش برای پیدا کردن پراپرتی (حتی اگر Private باشد)
                // BindingFlags.DeclaredOnly یعنی فقط در همین کلاس بگرد، اگر نبود برو کلاس پدر
                var prop = type.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (prop != null)
                {
                    prop.SetValue(obj, value, null);
                    return; // موفق شدیم، خروج
                }

                // 2. تلاش برای پیدا کردن فیلد پشتیبان (Backing Field)
                // معمولاً نامش <Name>k__BackingField است
                var field = type.GetField($"<{propertyName}>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (field != null)
                {
                    field.SetValue(obj, value);
                    return; // موفق شدیم، خروج
                }

                // اگر در این کلاس نبود، برو یک پله بالاتر (کلاس پدر)
                type = type.BaseType;
            }
        }
    }
}
