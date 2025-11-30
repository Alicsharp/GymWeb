using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Command;
using Gtm.Contract.ArticleContract.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = ErrorOr.Error;

namespace ShopTest.ArticleApp.Command
{
    public class CreateArticleCommandHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleValidator> _mockValidator;
        private readonly Mock<IFileService> _mockFileService;
        private readonly CreateArticleCommandHandler _handler;

        public CreateArticleCommandHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockValidator = new Mock<IArticleValidator>();
            _mockFileService = new Mock<IFileService>();

            _handler = new CreateArticleCommandHandler(
                _mockArticleRepo.Object,
                _mockValidator.Object,
                _mockFileService.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationErrors_When_ValidationFails()
        {
            // Arrange
            var command = CreateValidCommand();
            var validationError = Error.Validation("Title", "Title is required");

            _mockValidator.Setup(x => x.ValidateCreateAsync(command.command))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);

            _mockFileService.Verify(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            _mockArticleRepo.Verify(x => x.AddAsync(It.IsAny<Article>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_ImageUploadFails()
        {
            // Arrange
            var command = CreateValidCommand();

            _mockValidator.Setup(x => x.ValidateCreateAsync(command.command))
                .ReturnsAsync(Result.Success);

            // شبیه‌سازی شکست آپلود (null)
            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Description.Should().Contain("بارگزاری عکس");

            _mockArticleRepo.Verify(x => x.AddAsync(It.IsAny<Article>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_CleanupImages_And_ReturnError_When_DatabaseSaveFails()
        {
            // Arrange
            var command = CreateValidCommand();
            var uploadedImageName = "test-image.jpg";

            _mockValidator.Setup(x => x.ValidateCreateAsync(command.command)).ReturnsAsync(Result.Success);

            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(uploadedImageName);

            // ✅ اصلاح شد: برگرداندن true
            _mockFileService.Setup(x => x.ResizeImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // شبیه‌سازی شکست ذخیره در دیتابیس (false)
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Article.SaveFailed");

            // بررسی اینکه آیا عکس‌ها پاک شدند؟
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(uploadedImageName))), Times.Exactly(3));
        }

        [Fact]
        public async Task Handle_Should_CreateArticle_And_ReturnSuccess_When_EverythingIsOk()
        {
            // Arrange
            var command = CreateValidCommand();
            var uploadedImageName = "final-image.jpg";

            _mockValidator.Setup(x => x.ValidateCreateAsync(command.command)).ReturnsAsync(Result.Success);
            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync(uploadedImageName);

            // ✅ اصلاح شد: برگرداندن true
            _mockFileService.Setup(x => x.ResizeImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            _mockArticleRepo.Verify(x => x.AddAsync(It.Is<Article>(a =>
                a.Title == command.command.Title &&
                a.ImageName == uploadedImageName
            )), Times.Once);

            _mockArticleRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private CreateArticleCommand CreateValidCommand()
        {
            var dto = new CreateArticleDto
            {
                Title = "Test Article",
                Slug = "test-article",
                ShortDescription = "Short Desc",
                Text = "Full Text Content",
                ImageAlt = "Alt Text",
                CategoryId = 1,
                SubCategoryId = 2,
                UserId = 100,
                Writer = "Ali",
                ImageFile = new Mock<IFormFile>().Object
            };
            return new CreateArticleCommand(dto);
        }
    }
}
 