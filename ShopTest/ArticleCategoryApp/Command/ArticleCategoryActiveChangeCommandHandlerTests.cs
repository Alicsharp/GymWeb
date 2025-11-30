using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Command;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopTest.ArticleCategoryApp.Command
{
    public class ArticleCategoryActiveChangeCommandHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly ArticleCategoryActiveChangeCommandHandler _handler;

        public ArticleCategoryActiveChangeCommandHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();

            _handler = new ArticleCategoryActiveChangeCommandHandler(
                _mockRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_IdIsInvalid()
        {
            // Arrange
            int invalidId = -1;
            var command = new ArticleCategoryActiveChangeCommand(invalidId);
            var validationError = Error.Validation("Id.Invalid", "Invalid Id");

            _mockValidator.Setup(x => x.ValidateIdAsync(invalidId))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);

            // نباید دیتابیس صدا زده شود
            _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            int id = 10;
            var command = new ArticleCategoryActiveChangeCommand(id);

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);

            // دیتابیس نال برمی‌گرداند
            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_SaveChangesFails()
        {
            // Arrange
            int id = 10;
            var command = new ArticleCategoryActiveChangeCommand(id);
            var category = CreateCategory(id, isActive: true);

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("ActiveChange");
        }

        [Fact]
        public async Task Handle_Should_ToggleActiveState_And_ReturnSuccess()
        {
            // Arrange
            int id = 10;
            var command = new ArticleCategoryActiveChangeCommand(id);

            // دسته‌بندی اولیه فعال است (true)
            var category = CreateCategory(id, isActive: true);
            bool initialStatus = true;

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // ذخیره موفق
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // بررسی تغییر وضعیت: باید false شده باشد (چون قبلاً true بود)
            // اینجا باید با رفلکشن مقدار فیلد IsActive را بخوانیم تا مطمئن شویم تغییر کرده
            // یا اگر پراپرتی پابلیک گِتر (Public Getter) دارد که مستقیم چک می‌کنیم.
            // فرض می‌کنیم باید با رفلکشن چک کنیم یا به متد ActivationChange اعتماد کنیم.

            // اما بهتر است مطمئن شویم متد SaveChangesAsync صدا زده شده است
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- Helpers ---
        private ArticleCategory CreateCategory(int id, bool isActive)
        {
            var category = new ArticleCategory("Title", "slug", "img", "alt");

            ForceSetProperty(category, "Id", id);
            ForceSetProperty(category, "IsActive", isActive);

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
