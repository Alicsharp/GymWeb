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
using static System.Net.Mime.MediaTypeNames;

namespace ShopTest.ArticleCategoryApp.Command
{
    public class CreateArticleCategoryCommandHandlerTests
    {
        private readonly Mock<IArticleCategoryRepo> _mockRepo;
        private readonly Mock<IArticleCategoryValidator> _mockValidator;
        private readonly Mock<IFileService> _mockFileService;
        private readonly CreateArticleCategoryCommandHandler _handler;

        public CreateArticleCategoryCommandHandlerTests()
        {
            _mockRepo = new Mock<IArticleCategoryRepo>();
            _mockValidator = new Mock<IArticleCategoryValidator>();
            _mockFileService = new Mock<IFileService>();

            _handler = new CreateArticleCategoryCommandHandler(
                _mockRepo.Object,
                _mockValidator.Object,
                _mockFileService.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_DtoIsInvalid()
        {
            // Arrange
            var dto = new CreateArticleCategory { Title = "" }; // نامعتبر
            var command = new CreateArticleCategoryCommand(dto);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto))
                .ReturnsAsync(Error.Validation("Title", "Required"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            _mockFileService.Verify(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnError_When_ParentHasParent_MaxDepthError() // نام تست را اصلاح کردم و پرانتز را خالی کردم
        {
            // Arrange
            // تلاش برای ساخت زیردسته برای دسته‌ای که خودش زیردسته است (سطح 3)
            var dto = new CreateArticleCategory { Title = "Level 3", Parent = 20 };
            var command = new CreateArticleCategoryCommand(dto);

            // والد (آیدی 20) خودش یک والد دارد (آیدی 10)
            var parentCategory = CreateCategory(20, parentId: 10);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);

            _mockRepo.Setup(x => x.GetByIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentCategory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Parent.Invalid");

            // نباید عکسی آپلود شود
            _mockFileService.Verify(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_PerformRollback_When_SaveFails()
        {
            // Arrange
            var dto = new CreateArticleCategory { Title = "New Cat", ImageFile = new Mock<IFormFile>().Object };
            var command = new CreateArticleCategoryCommand(dto);
            string uploadedImageName = "new-image.jpg";

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);

            // آپلود موفق
            _mockFileService.Setup(x => x.UploadImageAsync(dto.ImageFile, It.IsAny<string>()))
                .ReturnsAsync(uploadedImageName);

            // ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.AddAsync(It.IsAny<ArticleCategory>())); // فقط ادد میشه
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Category.SaveFailed");

            // 

            //[Image of Rollback Diagram]
            // ->سیستم باید عکس آپلود شده را پاک کند
            // بررسی پاک شدن عکس (Rollback)
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(uploadedImageName))), Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handle_Should_CreateCategory_When_EverythingIsOk()
        {
            // Arrange
            var dto = new CreateArticleCategory { Title = "Root Cat", ImageFile = new Mock<IFormFile>().Object };
            var command = new CreateArticleCategoryCommand(dto);
            string uploadedImageName = "final.jpg";

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);
            _mockFileService.Setup(x => x.UploadImageAsync(dto.ImageFile, It.IsAny<string>())).ReturnsAsync(uploadedImageName);

            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().BeTrue();

            // بررسی ذخیره در دیتابیس
            _mockRepo.Verify(x => x.AddAsync(It.Is<ArticleCategory>(c =>
                c.Title == dto.Title &&
                c.ImageName == uploadedImageName
            )), Times.Once);

            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- Helper Methods ---
        private ArticleCategory CreateCategory(int id, int? parentId = null)
        {
            var cat = new ArticleCategory("Title", "slug", "img", "alt", parentId);
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
