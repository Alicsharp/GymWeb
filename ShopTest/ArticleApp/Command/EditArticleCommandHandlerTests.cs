using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Command;
using Gtm.Contract.ArticleContract.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using Microsoft.AspNetCore.Http;
using Moq;
using Utility.Appliation.FileService;

namespace ShopTest.ArticleApp.Command
{
    public class EditArticleCommandHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IArticleValidator> _mockValidator;
        private readonly EditArticleCommandHandler _handler;

        public EditArticleCommandHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockFileService = new Mock<IFileService>();
            _mockValidator = new Mock<IArticleValidator>();

            _handler = new EditArticleCommandHandler(
                _mockArticleRepo.Object,
                _mockFileService.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_ArticleDoesNotExist()
        {
            // Arrange
            var command = CreateCommand();

            // ولیدیشن پاس شود
            _mockValidator.Setup(x => x.ValidateUpdateAsync(command.command)).ReturnsAsync(Result.Success);

            // مقاله پیدا نشود
            _mockArticleRepo.Setup(x => x.GetByIdAsync(command.command.Id, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Article?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_UpdateArticle_And_DeleteOldImage_When_NewImageUploaded_And_SaveSucceeds()
        {
            // Arrange (سناریوی موفقیت با تغییر عکس)
            var command = CreateCommand(withImage: true);
            var oldImageName = "old-pic.jpg";
            var newImageName = "new-pic.jpg";

            var article = CreateArticle(oldImageName);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(command.command)).ReturnsAsync(Result.Success);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(command.command.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(article);

            // آپلود عکس جدید
            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(newImageName);

            // ریسایز موفق
            _mockFileService.Setup(x => x.ResizeImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // ذخیره موفق
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // 1. بررسی کنیم که مقاله با عکس *جدید* ادیت شده باشد
            article.ImageName.Should().Be(newImageName);
            article.Title.Should().Be(command.command.Title);

            // 2. بررسی کنیم که عکس *قدیمی* پاک شده باشد (Cleanup)
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(oldImageName))), Times.AtLeastOnce());

            // 3. بررسی کنیم که عکس *جدید* اشتباهاً پاک نشده باشد
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(newImageName))), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_DeleteNewUploadedImage_When_SaveFails()
        {
            // Arrange (سناریوی رول‌بک: آپلود شده ولی ذخیره نشده)
            var command = CreateCommand(withImage: true);
            var oldImageName = "old-pic.jpg";
            var newImageName = "new-pic.jpg"; // عکسی که آپلود میشه ولی باید پاک شه

            var article = CreateArticle(oldImageName);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(command.command)).ReturnsAsync(Result.Success);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(command.command.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(article);

            // آپلود و ریسایز موفق
            _mockFileService.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync(newImageName);
            _mockFileService.Setup(x => x.ResizeImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

            // 🚨 ذخیره شکست می‌خورد
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Blog.SaveFailed");

            // *** بخش حیاتی: بررسی Rollback ***
            // باید عکس *جدید* که همین الان آپلود شد را پاک کند تا سرور کثیف نشود
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(newImageName))), Times.AtLeastOnce());

            // نباید عکس *قدیمی* را پاک کند (چون ادیت انجام نشد)
            _mockFileService.Verify(x => x.DeleteImageAsync(It.Is<string>(s => s.Contains(oldImageName))), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_UpdateInfo_WithoutTouchingImages_When_NoImageProvided()
        {
            // Arrange (سناریوی ادیت متنی ساده)
            var command = CreateCommand(withImage: false); // عکس نال است
            var oldImageName = "keep-me.jpg";
            var article = CreateArticle(oldImageName);

            _mockValidator.Setup(x => x.ValidateUpdateAsync(command.command)).ReturnsAsync(Result.Success);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(command.command.Id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(article);
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // عکس نباید عوض شده باشد
            article.ImageName.Should().Be(oldImageName);
            // تایتل باید عوض شده باشد
            article.Title.Should().Be(command.command.Title);

            // هیچ عکسی نباید آپلود شده باشد
            _mockFileService.Verify(x => x.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
            // هیچ عکسی نباید حذف شده باشد
            _mockFileService.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
        }

        // --- Helpers ---
        private EditArticleCommand CreateCommand(bool withImage = false)
        {
            var dto = new UpdateArticleDto
            {
                Id = 1,
                Title = "New Title",
                Slug = "new-slug",
                ShortDescription = "Desc",
                Text = "Text",
                ImageAlt = "Alt",
                CategoryId = 1,
                SubCategoryId = 2,
                Writer = "Writer",
                ImageFile = withImage ? new Mock<IFormFile>().Object : null
            };
            return new EditArticleCommand(dto);
        }

        private Article CreateArticle(string imageName)
        {
            // ساخت یک مقاله فیک با وضعیت اولیه
            return new Article(
                "Old Title", "old-slug", "Old Desc", "Old Text", imageName, "Old Alt",
                1, 2, 100, "Old Writer"
            );
        }
    }
}
 