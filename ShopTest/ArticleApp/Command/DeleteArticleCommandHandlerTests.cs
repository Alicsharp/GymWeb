using ErrorOr;
using FluentAssertions;
using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleApp.Command;
using Moq;

namespace ShopTest.ArticleApp.Command
{
    public class DeleteArticleCommandHandlerTests
    {
        private readonly Mock<IArticleRepo> _mockArticleRepo;
        // نکته: حتی اگر در کد استفاده نشده، چون در Constructor هست باید ماک شود
        private readonly Mock<IArticleValidator> _mockValidator;

        private readonly DeleteArticleCommandHandler _handler;

        public DeleteArticleCommandHandlerTests()
        {
            _mockArticleRepo = new Mock<IArticleRepo>();
            _mockValidator = new Mock<IArticleValidator>();

            _handler = new DeleteArticleCommandHandler(
                _mockArticleRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_When_ArticleIsDeletedSuccessfully()
        {
            // Arrange
            int articleId = 10;
            var command = new DeleteArticleCommand(articleId);

            // سناریو: ریپازیتوری میگه "باشه، حذف کردم" (true)
            _mockArticleRepo.Setup(x => x.RemoveByIdAsync(articleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // اطمینان حاصل کنیم که متد حذف با آیدی درست صدا زده شده
            _mockArticleRepo.Verify(x => x.RemoveByIdAsync(articleId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_DeletionFails()
        {
            // Arrange
            int articleId = 999; // آیدی الکی
            var command = new DeleteArticleCommand(articleId);

            // سناریو: ریپازیتوری میگه "نتونستم حذف کنم" (false)
            // (مثلاً رکورد پیدا نشد یا دیتابیس قفل بود)
            _mockArticleRepo.Setup(x => x.RemoveByIdAsync(articleId, It.IsAny<CancellationToken>()))
     .ReturnsAsync(false);
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();

            // چک کردن کد خطایی که در هندلر نوشتی
            result.FirstError.Code.Should().Be("DeleteArticle");
            result.FirstError.Description.Should().Be("عملیات حذف شکست خورد");

            // مطمئن شویم که تلاش برای حذف انجام شده (حتی اگر شکست خورده)
            // ✅ درست: اضافه کردن It.IsAny<CancellationToken>()
            _mockArticleRepo.Verify(x => x.RemoveByIdAsync(articleId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
 