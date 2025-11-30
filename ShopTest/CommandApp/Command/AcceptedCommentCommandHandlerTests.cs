using ErrorOr;
using FluentAssertions;
using Gtm.Application.CommentApp;
using Gtm.Application.CommentApp.Command;
using Gtm.Domain.CommentDomain;
using Moq;
using System.Reflection;
using Utility.Domain.Enums;

namespace ShopTest.CommandApp.Command
{
    public class AcceptedCommentCommandHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockRepo;
        private readonly AcceptedCommentCommandHandler _handler;

        public AcceptedCommentCommandHandlerTests()
        {
            _mockRepo = new Mock<ICommentRepo>();
            _handler = new AcceptedCommentCommandHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_CommentDoesNotExist()
        {
            // Arrange
            var command = new AcceptedCommentCommand(100);

            _mockRepo.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Comment?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);

            // ذخیره نباید صدا زده شود
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_SaveChangesFails()
        {
            // Arrange
            var command = new AcceptedCommentCommand(100);
            var comment = CreateComment(100, CommentStatus.خوانده_نشده);

            _mockRepo.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comment);

            // ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Database.SaveError");
        }

        [Fact]
        public async Task Handle_Should_ApproveComment_And_ReturnSuccess()
        {
            // Arrange
            var command = new AcceptedCommentCommand(100);
            // کامنت اولیه وضعیتش "خوانده نشده" است
            var comment = CreateComment(100, CommentStatus.خوانده_نشده);

            _mockRepo.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comment);

            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // 1. بررسی اینکه متد SaveChanges صدا زده شده
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. بررسی اینکه وضعیت کامنت واقعاً تغییر کرده است
            comment.Status.Should().Be(CommentStatus.تایید_شده);
        }

        // --- Helper Methods ---
        private Comment CreateComment(long id, CommentStatus status)
        {
            // ساخت کامنت با وضعیت اولیه دلخواه
            var comment = new Comment(1, 1, CommentFor.مقاله, "Name", "Mail", "Text", null);
            ForceSetProperty(comment, "Id", id);
            ForceSetProperty(comment, "Status", status);
            return comment;
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
