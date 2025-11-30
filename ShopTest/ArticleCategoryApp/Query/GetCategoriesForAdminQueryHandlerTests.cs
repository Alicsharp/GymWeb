using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using MockQueryable;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopTest.ArticleCategoryApp.Query
{
    public class GetCategoriesForAdminQueryHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly GetCategoriesForAdminQueryHandler _handler;

        public GetCategoriesForAdminQueryHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();

            _handler = new GetCategoriesForAdminQueryHandler(
                _mockRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnRootCategories_When_IdIsZero()
        {
            // Arrange
            var query = new GetCategoriesForAdminQuery(0);

            // لیست شامل ریشه و فرزند
            var categories = new List<ArticleCategory>
            {
                CreateCategory(1, "Root 1", null),       // باید بیاید
                CreateCategory(2, "Root 2", null),       // باید بیاید
                CreateCategory(3, "Child 1", 1)          // نباید بیاید
            };

            var mockDbSet = categories.BuildMock();

            // ستاپ کوئری
            _mockRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<ArticleCategory, bool>>>()))
                .Returns(mockDbSet);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.PageTitle.Should().Be("لیست سر دسته‌های مقاله");

            // باید 2 تا ریشه برگردد
            result.Value.articleCategories.Should().HaveCount(2);
            result.Value.articleCategories.Select(c => c.Id).Should().Contain(new[] { 1, 2 });
            result.Value.articleCategories.Select(c => c.Id).Should().NotContain(3);
        }

        [Fact]
        public async Task Handle_Should_ReturnSubCategories_When_IdIsPositive_And_ParentExists()
        {
            // Arrange
            int parentId = 10;
            var query = new GetCategoriesForAdminQuery(parentId);
            var parentCat = CreateCategory(parentId, "Parent Cat", null);

            var categories = new List<ArticleCategory>
            {
                CreateCategory(10, "Parent Cat", null),
                CreateCategory(11, "Child A", 10), // باید بیاید
                CreateCategory(12, "Child B", 10), // باید بیاید
                CreateCategory(13, "Other Root", null) // نباید بیاید
            };

            var mockDbSet = categories.BuildMock();

            _mockValidator.Setup(x => x.ValidateIdAsync(parentId)).ReturnsAsync(Result.Success);

            // پیدا شدن والد
            _mockRepo.Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentCat);

            // ستاپ کوئری (نکته: در هندلر ما شرط Where میزنیم، پس اینجا کل لیست را برمی‌گردانیم تا LINQ کار کند)
            _mockRepo.Setup(x => x.QueryBy(It.IsAny<Expression<Func<ArticleCategory, bool>>>()))
                .Returns(mockDbSet);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.PageTitle.Should().Contain("Parent Cat");

            // باید فقط فرزندانِ ParentId=10 برگردند
            result.Value.articleCategories.Should().HaveCount(2);
            result.Value.articleCategories.First().Title.Should().Be("Child A");
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_ParentIdDoesNotExist()
        {
            // Arrange
            int parentId = 99;
            var query = new GetCategoriesForAdminQuery(parentId);

            _mockValidator.Setup(x => x.ValidateIdAsync(parentId)).ReturnsAsync(Result.Success);

            // والد پیدا نشود
            _mockRepo.Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        // --- Helper Methods ---
        private ArticleCategory CreateCategory(int id, string title, int? parentId)
        {
            var category = new ArticleCategory(title, "slug", "img.jpg", "alt", parentId);
            ForceSetProperty(category, "Id", id);
            ForceSetProperty(category, "CreateDate", DateTime.Now);
            ForceSetProperty(category, "UpdateDate", DateTime.Now);
            ForceSetProperty(category, "IsActive", true);
            return category;
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
