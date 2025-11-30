using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Application.ArticleCategoryApp.Command;
using Gtm.Contract.ArticleCategoryContract.Command;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using Utility.Appliation.FileService;

namespace ShopTest.ArticleCategoryApp.Command
{
    public class EditArticleCategoryCommandHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly Mock<IFileService> _mockFileService;
        private readonly EditArticleCategoryCommandHandler _handler;

        public EditArticleCategoryCommandHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();
            _mockFileService = new Mock<IFileService>();

            _handler = new EditArticleCategoryCommandHandler(
                _mockRepo.Object,
                _mockValidator.Object,
                _mockFileService.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
        {
            // Arrange
            var dto = new UpdateArticleCategoryDto { Id = 1, Title = "Edit" };
            var command = new EditArticleCategoryCommand(dto);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(dto)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ArticleCategory?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_UpdateInfo_And_DeleteOldImage_When_ImageChanged_And_SaveSucceeds()
        {
            // Arrange
            var dto = new UpdateArticleCategoryDto
            {
                Id = 1,
                Title = "New Title",
                ImageFile = new Mock<IFormFile>().Object // ارسال فایل
            };
            var command = new EditArticleCategoryCommand(dto);

            string oldImage = "old.jpg";
            string newImage = "new.jpg";

            // دسته‌بندی موجود
            var category = CreateCategory(1, oldImage);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(dto)).ReturnsAsync(Result.Success);
            _mockRepo.Setup(x => x.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            // آپلود عکس جدید
            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(newImage);

            // ذخیره موفق
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // 1. بررسی آپدیت شدن انتیتی
            // (توجه: چون متد Update در انتیتی void است و پراپرتی‌ها private set هستند، 
            // در تست واقعی سخت است چک کنیم مگر اینکه Getter پابلیک داشته باشد.
            // اما می‌توانیم مطمئن شویم SaveChanges صدا زده شده)
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. بررسی پاک شدن عکس قدیمی
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(oldImage))), Times.AtLeastOnce());

            // 3. عکس جدید نباید پاک شود
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(newImage))), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_RollbackNewImage_When_SaveFails()
        {
            // Arrange
            var dto = new UpdateArticleCategoryDto
            {
                Id = 1,
                Title = "New Title",
                ImageFile = new Mock<IFormFile>().Object
            };
            var command = new EditArticleCategoryCommand(dto);

            string oldImage = "old.jpg";
            string newImage = "new.jpg"; // عکسی که آپلود میشه ولی باید پاک شه

            var category = CreateCategory(1, oldImage);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(dto)).ReturnsAsync(Result.Success);
            _mockRepo.Setup(x => x.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync(newImage);

            // 🚨 ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Database.SaveError");

            // *** بررسی Rollback ***
            // عکس جدید باید پاک شود
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(newImage))), Times.AtLeastOnce());

            // عکس قدیمی نباید پاک شود
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(oldImage))), Times.Never);
        }

        // --- Helpers ---
        private ArticleCategory CreateCategory(int id, string imageName)
        {
            var cat = new ArticleCategory("Title", "slug", imageName, "alt");
            ForceSetProperty(cat, "Id", id);
            return cat;
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
