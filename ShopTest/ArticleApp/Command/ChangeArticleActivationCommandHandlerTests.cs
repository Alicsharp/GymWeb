using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using Moq;
using Error = ErrorOr.Error;

namespace ShopTest.ArticleApp.Command
{
    public class ChangeArticleActivationCommandHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        private readonly Mock<IArticleValidator> _mockValidator;
        private readonly ChangeArticleActivationCommandHandler _handler;

        public ChangeArticleActivationCommandHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockValidator = new Mock<IArticleValidator>();

            _handler = new ChangeArticleActivationCommandHandler(
                _mockArticleRepo.Object,
                _mockValidator.Object
            );
        }

        // سناریوی ۱: آیدی نامعتبر است (مثلاً منفی)
        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_IdIsInvalid()
        {
            // Arrange
            int invalidId = -1;
            var command = new ChangeArticleActivationCommand(invalidId);
            var validationError = Error.Validation("Category.Id.Invalid", "شناسه نامعتبر");

            // ولیدیتور خطا برمی‌گرداند
            _mockValidator.Setup(x => x.ValidateIdAsync(invalidId))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);

            _mockArticleRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // سناریوی ۲: آیدی معتبر است، اما مقاله در دیتابیس پیدا نمی‌شود (404 Not Found)
        [Fact]
        public async Task Handle_Should_ReturnNotFoundError_When_ArticleDoesNotExist()
        {
            // Arrange
            int validId = 100; // آیدی ظاهرش درسته
            var command = new ChangeArticleActivationCommand(validId);

            _mockValidator.Setup(x => x.ValidateIdAsync(validId)).ReturnsAsync(Result.Success);

            _mockArticleRepo.Setup(x => x.GetByIdAsync(validId, It.IsAny<CancellationToken>()))
         .ReturnsAsync((Article?)null); // یا هر مقداری که قراره برگردونه
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound); // نوع خطا باید NotFound باشد
            result.FirstError.Code.Should().Be("Article.NotFound");

            // چون پیدا نشد، نباید تلاش کند چیزی ذخیره کند (جلوگیری از کرش)
            _mockArticleRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // سناریوی ۳: مقاله پیدا شد، اما ذخیره در دیتابیس شکست خورد
        [Fact]
        public async Task Handle_Should_ReturnFailure_When_SaveChangesFails()
        {
            // Arrange
            int id = 10;
            var command = new ChangeArticleActivationCommand(id);
            var article = CreateArticle();

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);
      _mockArticleRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(article);

            // ذخیره شکست می‌خورد (false)
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("NotSaved");

            // مطمئن شویم متدها صدا زده شده‌اند
            _mockArticleRepo.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _mockArticleRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // سناریوی ۴: همه چیز عالی پیش می‌رود (Happy Path)
        [Fact]
        public async Task Handle_Should_FlipActivationStatus_And_ReturnSuccess_When_EverythingIsOk()
        {
            // Arrange
            int id = 10;
            var command = new ChangeArticleActivationCommand(id);
            var article = CreateArticle();

            // وضعیت اولیه را ذخیره می‌کنیم تا بعدا مقایسه کنیم
            bool initialStatus = article.IsActive;

            _mockValidator.Setup(x => x.ValidateIdAsync(id)).ReturnsAsync(Result.Success);
            _mockArticleRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
          .ReturnsAsync(article);

            // ذخیره موفق (true)
            _mockArticleRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // 1. نتیجه باید موفق باشد
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // 2. وضعیت مقاله باید برعکس شده باشد (Toggling)
            article.IsActive.Should().NotBe(initialStatus);

            // 3. باید ذخیره شده باشد
            _mockArticleRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // متد کمکی برای ساخت دیتای فیک
        private Article CreateArticle()
        {
            // فرض می‌کنیم یک مقاله کامل می‌سازیم
            return new Article(
                "Test Title", "slug", "Short Desc", "Full Text", "image.jpg", "alt",
                1, 2, 100, "Writer Name"
            );
        }
    }
}
 