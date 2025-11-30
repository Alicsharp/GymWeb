using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Moq;
using System.Reflection;

namespace ShopTest.ArticleCategoryApp.Query
{
    public class GetArticleCategoryForEditQueryHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly GetArticleCategoryForEditQueryHandler _handler;

        public GetArticleCategoryForEditQueryHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();

            _handler = new GetArticleCategoryForEditQueryHandler(
                _mockRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_IdIsInvalid()
        {
            // Arrange
            var query = new GetArticleCategoryForEditQuery(-1);
            var validationError = Error.Validation("Id.Invalid", "Invalid Id");

            _mockValidator.Setup(x => x.ValidateIdAsync(-1))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);
            _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            var query = new GetArticleCategoryForEditQuery(10);

            _mockValidator.Setup(x => x.ValidateIdAsync(10)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_ReturnDto_WithParentIdZero_When_CategoryIsRoot()
        {
            // Arrange
            var query = new GetArticleCategoryForEditQuery(10);

            // دسته‌بندی ریشه (ParentId = null)
            var category = CreateCategory(10, "Root Cat", parentId: null);

            _mockValidator.Setup(x => x.ValidateIdAsync(10)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Title.Should().Be("Root Cat");

            // ✅ نکته مهم: اگر نال بود باید تبدیل به 0 شده باشد
            result.Value.Parent.Should().Be(0);
        }

        [Fact]
        public async Task Handle_Should_ReturnDto_WithCorrectParentId_When_CategoryIsChild()
        {
            // Arrange
            var query = new GetArticleCategoryForEditQuery(20);

            // دسته‌بندی فرزند (ParentId = 5)
            var category = CreateCategory(20, "Child Cat", parentId: 5);

            _mockValidator.Setup(x => x.ValidateIdAsync(20)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Parent.Should().Be(5);
        }

        // --- Helper Methods ---
        private ArticleCategory CreateCategory(int id, string title, int? parentId)
        {
            var category = new ArticleCategory(title, "slug", "img", "alt", parentId);
            ForceSetProperty(category, "Id", id);
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
