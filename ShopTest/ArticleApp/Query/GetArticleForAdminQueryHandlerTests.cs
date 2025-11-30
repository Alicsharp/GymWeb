using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Query;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopTest.ArticleApp.Query
{
    public class GetArticleForAdminQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleValidator> _mockValidator; // اینجا شاید خیلی نیاز نباشه ولی چون تو سازنده هست میذاریم
        private readonly Mock<IArticleCategoryRepo> _mockCategoryRepo;
        private readonly GetArticleForAdminQueryHandler _handler;

        public GetArticleForAdminQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockValidator = new Mock<IArticleValidator>();
            _mockCategoryRepo = new Mock<IArticleCategoryRepo>();

            _handler = new GetArticleForAdminQueryHandler(
                _mockArticleRepo.Object,
                _mockValidator.Object,
                _mockCategoryRepo.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnAllArticles_When_IdIsZero()
        {
            // Arrange
            var query = new GetArticleForAdminQuery(0);

            // ساخت دیتای فیک داخل رم
            var articles = new List<Article>
            {
                CreateArticle(1, "Art1", 10), // CatId 10
                CreateArticle(2, "Art2", 20)  // CatId 20
            };

            // تبدیل لیست به IQueryable قابل تست (جادوی MockQueryable)
            var mockDbSet = articles.AsQueryable().BuildMockDbSet();

            // تنظیم رفتار ریپازیتوری
            _mockArticleRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.PageTitle.Should().Be("لیست تمام مقالات");
            result.Value.Article.Should().HaveCount(2); // باید هر دو مقاله را برگرداند
        }

        [Fact]
        public async Task Handle_Should_FilterArticles_When_IdIsProvided()
        {
            // Arrange
            int targetCategoryId = 50;
            var query = new GetArticleForAdminQuery(targetCategoryId);

            var articles = new List<Article>
            {
                CreateArticle(1, "Art1", 50), // ✅ باید انتخاب شود
                CreateArticle(2, "Art2", 50), // ✅ باید انتخاب شود
                CreateArticle(3, "Art3", 100) // ❌ نباید انتخاب شود
            };

            var mockDbSet = articles.AsQueryable().BuildMock();
            _mockArticleRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet);

            // باید دسته‌بندی را هم ماک کنیم تا تایتل صفحه درست در بیاید
            var category = new ArticleCategory("Sport", "sport", "img", "alt");
            // ✅ درست: پارامتر دوم را با It.IsAny پر می‌کنیم
            _mockCategoryRepo.Setup(x => x.GetByIdAsync(
                    targetCategoryId,
                    It.IsAny<CancellationToken>() // <--- این بخش اضافه شد
                ))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // چک می‌کنیم تایتل بر اساس دسته‌بندی باشد
            result.Value.PageTitle.Should().Contain("Sport");

            // مهم: چک می‌کنیم فیلترینگ درست کار کرده باشد (فقط ۲ تا مقاله با آیدی ۵۰)
            result.Value.Article.Should().HaveCount(2);
            result.Value.Article.All(a => a.CategoryId == targetCategoryId).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CategoryIdDoesNotExist()
        {
            // Arrange
            int invalidCatId = 999;
            var query = new GetArticleForAdminQuery(invalidCatId);

            // دیتابیس مقاله مهم نیست، ولی ستاپ می‌کنیم که خالی نباشد
            var mockDbSet = new List<Article>().AsQueryable().BuildMock();
            _mockArticleRepo.Setup(x => x.GetAllQueryable()).Returns(mockDbSet);

            // دسته‌بندی پیدا نمی‌شود (null)
            _mockCategoryRepo.Setup(x => x.GetByIdAsync(
            invalidCatId,
            It.IsAny<CancellationToken>() // <--- این بخش را اضافه کن
        ))
        .ReturnsAsync((ArticleCategory?)null);
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        // --- Helper ---
        private Article CreateArticle(int id, string title, int catId)
        {
            // استفاده از Reflection برای ست کردن Id (چون معمولا ست‌رِ Id پرایوت است)
            var article = new Article(title, "slug", "desc", "text", "img", "alt", catId, 0, 1, "writer");

            // ترفند برای ست کردن Id در تست وقتی Setter خصوصی است
            article.GetType().GetProperty("Id")?.SetValue(article, id);

            // تنظیم تاریخ برای اینکه ToPersianDate خطا ندهد (اگر اکستنشن متد روی دیت تایم دارید)
            // article.GetType().GetProperty("CreateDate")?.SetValue(article, DateTime.Now);

            return article;
        }
    }
}
