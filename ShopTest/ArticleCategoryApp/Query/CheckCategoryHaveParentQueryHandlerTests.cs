using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopTest.ArticleCategoryApp.Query
{
    public class CheckCategoryHaveParentQueryHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly CheckCategoryHaveParentQueryHandler _handler;

        public CheckCategoryHaveParentQueryHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();

            _handler = new CheckCategoryHaveParentQueryHandler(
                _mockRepo.Object,
                _mockValidator.Object
            );
        }

        // ----------------------------------------------------------------
        // روش حرفه‌ای: استفاده از Theory برای چک کردن چندین سناریوی خطا
        // ----------------------------------------------------------------

        [Theory]
        [InlineData(-1, true)]  // سناریو 1: آیدی نامعتبر (توسط ولیدیتور رد میشه)
        [InlineData(0, true)]   // سناریو 2: آیدی صفر (توسط ولیدیتور رد میشه)
        public async Task Handle_Should_ReturnError_When_IdIsInvalid(int id, bool isValidationError)
        {
            // Arrange
            var query = new CheckCategoryHaveParentQuery(id);
            var validationError = Error.Validation("Id.Invalid", "ID Invalid");

            // تنظیم ولیدیتور بر اساس ورودی تئوری
            if (isValidationError)
            {
                _mockValidator.Setup(x => x.ValidateIdAsync(id))
                    .ReturnsAsync(validationError);
            }

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.Validation);

            // دیتابیس نباید صدا زده شود
            _mockRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact] // این چون یک حالت خاص (NotFound) است، Fact باقی می‌ماند (Mock متفاوت دارد)
        public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            var query = new CheckCategoryHaveParentQuery(10);
            _mockValidator.Setup(x => x.ValidateIdAsync(10)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        // ----------------------------------------------------------------
        // روش حرفه‌ای: استفاده از Theory برای چک کردن منطق والد/فرزند
        // ----------------------------------------------------------------

        [Theory]
        [InlineData(10, null, false)] // والد ندارد (ParentId = null) -> باید Success باشد
        [InlineData(20, 10, true)]    // والد دارد (ParentId = 10) -> باید Error Failure باشد
        public async Task Handle_Should_ReturnExpectedResult_BasedOnParentStatus(int id, int? parentId, bool expectFailure)
        {
            // Arrange
            var query = new CheckCategoryHaveParentQuery(id);

            // ساخت کتگوری با ParentId متفاوت بر اساس ورودی Theory
            var category = CreateCategory(id, parentId);

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            if (expectFailure)
            {
                result.IsError.Should().BeTrue();
                result.FirstError.Type.Should().Be(ErrorType.Failure);
                // result.FirstError.Code.Should().Be("Category.IsChild");
            }
            else
            {
                result.IsError.Should().BeFalse();
                result.Value.Should().Be(Result.Success);
            }
        }

        // --- Helper Methods ---
        private ArticleCategory CreateCategory(int id, int? parentId)
        {
            var category = new ArticleCategory("Title", "slug", "img", "alt", parentId);
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
