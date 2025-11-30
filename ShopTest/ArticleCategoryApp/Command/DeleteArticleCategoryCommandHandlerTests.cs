using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Command;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Gtm.Domain.BlogDomain.BlogDm;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace ShopTest.ArticleCategoryApp.Command
{
    public class DeleteArticleCategoryCommandHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockCategoryRepo;
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly DeleteArticleCategoryCommandHandler _handler;

        public DeleteArticleCategoryCommandHandlerTests()
        {
            _mockCategoryRepo = new Mock<IArticleCategoryRepo>();
            _mockArticleRepo = new Mock<IArticleRepo>();

            _handler = new DeleteArticleCategoryCommandHandler(
                _mockCategoryRepo.Object,
                _mockArticleRepo.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            var command = new DeleteArticleCategoryCommand(1);

            // دسته‌بندی پیدا نمی‌شود
            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);

            // تراکنش نباید شروع شده باشد
            _mockCategoryRepo.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnConflict_When_CategoryHasArticles_And_ForceDeleteIsFalse()
        {
            // Arrange
            var command = new DeleteArticleCategoryCommand(1, ForceDelete: false);
            var category = CreateCategory(1, "Cat 1");

            // دسته‌بندی پیدا می‌شود
            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // مقاله دارد (Exists = true)
            _mockArticleRepo.Setup(x => x.ExistsAsync(
                    It.IsAny<Expression<Func<Article, bool>>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.Conflict);
            result.FirstError.Code.Should().Be("Category.HasArticles");

            // حذف نباید صدا زده شود
            _mockCategoryRepo.Verify(x => x.RemoveByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_DeleteArticlesAndCategory_When_ForceDeleteIsTrue()
        {
            // Arrange
            var command = new DeleteArticleCategoryCommand(1, ForceDelete: true);
            var category = CreateCategory(1, "Cat 1");

            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            // مقاله دارد
            _mockArticleRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Article, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Deleted);

            // بررسی جریان (Flow)
            // 1. شروع تراکنش
            _mockCategoryRepo.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. حذف مقالات (چون ForceDelete بود)
            _mockArticleRepo.Verify(x => x.DeleteByCategoryIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);

            // 3. حذف دسته
            _mockCategoryRepo.Verify(x => x.RemoveByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);

            // 4. کامیت
            _mockCategoryRepo.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_RollbackTransaction_When_ExceptionOccurs()
        {
            // Arrange
            var command = new DeleteArticleCategoryCommand(1, ForceDelete: true);
            var category = CreateCategory(1, "Cat 1");

            _mockCategoryRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            // فرض می‌کنیم در مرحله حذف دسته، دیتابیس ارور می‌دهد
            _mockCategoryRepo.Setup(x => x.RemoveByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB Connection Error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Database.DeleteError");

            // بررسی Rollback
            _mockCategoryRepo.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

            // بررسی اینکه Commit صدا زده نشده
            _mockCategoryRepo.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // --- Helper Methods ---
        private ArticleCategory CreateCategory(int id, string title)
        {
            var category = new ArticleCategory(title, "slug", "img", "alt");
            ForceSetProperty(category, "Id", id);
            return category;
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
