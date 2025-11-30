using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Query;
using Gtm.Domain.BlogDomain.BlogDm;
using Moq;
using System.Reflection;

namespace ShopTest.ArticleApp.Query
{
    public class GetForEditArticleQueryHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleValidator> _mockValidator;
        private readonly GetForEditArticleQueryHandler _handler;

        public GetForEditArticleQueryHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockValidator = new Mock<IArticleValidator>();

            _handler = new GetForEditArticleQueryHandler(
                _mockArticleRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_IdIsInvalid()
        {
            // Arrange
            int invalidId = -1;
            var query = new GetForEditArticleQuery(invalidId);
            var validationError = Error.Validation("Id.Invalid", "Invalid Id");

            // ولیدیتور خطا برمی‌گرداند
            _mockValidator.Setup(x => x.ValidateIdAsync(invalidId))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);

            // ریپازیتوری نباید صدا زده شود
            _mockArticleRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_ArticleDoesNotExist()
        {
            // Arrange
            int validId = 10;
            var query = new GetForEditArticleQuery(validId);

            // ولیدیشن اوکی است
            _mockValidator.Setup(x => x.ValidateIdAsync(validId))
                .ReturnsAsync(Result.Success);

            // دیتابیس نال برمی‌گرداند
            _mockArticleRepo.Setup(x => x.GetByIdAsync(validId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Article?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
            result.FirstError.Code.Should().Contain("NotFound");
        }

        [Fact]
        public async Task Handle_Should_ReturnDto_WithCorrectData_When_ArticleFound()
        {
            // Arrange
            int id = 100;
            var query = new GetForEditArticleQuery(id);

            // ساخت مقاله فیک
            var article = CreateArticle(id, "My Title", "Ali Writer", "Slug-1", "Desc", "Text Body");

            // ولیدیشن اوکی
            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);

            // دیتابیس مقاله را برمی‌گرداند
            _mockArticleRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(article);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // چک کردن اینکه مپینگ درست انجام شده (DTO برابر با Entity باشد)
            var dto = result.Value;
            dto.Id.Should().Be(id);
            dto.Title.Should().Be("My Title");
            dto.Writer.Should().Be("Ali Writer");
            dto.Slug.Should().Be("Slug-1");
            dto.ShortDescription.Should().Be("Desc");
            dto.Text.Should().Be("Text Body");
        }

        // --- Helper Methods ---

        private Article CreateArticle(int id, string title, string writer, string slug, string desc, string text)
        {
            var article = new Article(title, slug, desc, text, "img.jpg", "alt", 1, 0, 1, writer);

            // استفاده از متد قدرتمند برای ست کردن Id (چون در کلاس پدر است)
            ForceSetProperty(article, "Id", id);

            return article;
        }

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
