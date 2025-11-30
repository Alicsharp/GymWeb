using ErrorOr;
using FluentAssertions;
using Gtm.Application.CommentApp;
using Gtm.Application.CommentApp.Command;
using Gtm.Contract.CommentContract.Command;
using Gtm.Domain.CommentDomain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace ShopTest.CommandApp.Command
{
    public class CreateCommentCommandHandlerTests
    {
        private readonly Mock<ICommentRepo> _mockRepo;
        private readonly Mock<ICommentValidator> _mockValidator;
        private readonly CreateCommentCommandHandler _handler;

        public CreateCommentCommandHandlerTests()
        {
            _mockRepo = new Mock<ICommentRepo>();
            _mockValidator = new Mock<ICommentValidator>();

            _handler = new CreateCommentCommandHandler(
                _mockRepo.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_DtoIsInvalid()
        {
            // Arrange
            var dto = new CreateCommentDto { Text = "" }; // متن خالی
            var command = new CreateCommentCommand(dto);
            var validationError = Error.Validation("Text", "Empty");

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(validationError);

            // دیتابیس نباید صدا زده شود
            _mockRepo.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_ParentIdDoesNotExist()
        {
            // Arrange
            long parentId = 999;
            var dto = new CreateCommentDto
            {
                Text = "Reply",
                UserId = 1,
                OwnerId = 10,
                ParentId = parentId // این والد وجود ندارد
            };
            var command = new CreateCommentCommand(dto);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);

            // والد در دیتابیس پیدا نمی‌شود
            _mockRepo.Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Comment?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Comment.ParentNotFound");

            // نباید ادد شود
            _mockRepo.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_SaveChangesFails()
        {
            // Arrange
            var dto = new CreateCommentDto { Text = "Nice!", UserId = 1, OwnerId = 10 };
            var command = new CreateCommentCommand(dto);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);

            // ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Comment.SaveFailed");
        }

        [Fact]
        public async Task Handle_Should_CreateComment_When_InputsAreValid()
        {
            // Arrange
            var dto = new CreateCommentDto
            {
                Text = "Great Post",
                UserId = 100,
                OwnerId = 50,
                FullName = "Ali",
                Email = "ali@test.com",
                CommentFor = CommentFor.مقاله
            };
            var command = new CreateCommentCommand(dto);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // بررسی اینکه کامنت با مشخصات درست ساخته شده
            _mockRepo.Verify(x => x.AddAsync(It.Is<Comment>(c =>
                c.Text == dto.Text &&
                c.AuthorUserId == dto.UserId &&
                c.TargetEntityId == dto.OwnerId &&
                c.ParentId == null // چون والد نداشتیم
            )), Times.Once);

            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_CreateReplyComment_When_ParentExists()
        {
            // Arrange
            long parentId = 55;
            var dto = new CreateCommentDto
            {
                Text = "Thanks for info",
                UserId = 100,
                OwnerId = 50,
                ParentId = parentId // پاسخ به نظر
            };
            var command = new CreateCommentCommand(dto);

            // کامنت والد
            var parentComment = new Comment(1, 50, CommentFor.مقاله, "User", "e@e.com", "Main", null);
            ForceSetId(parentComment, parentId);

            _mockValidator.Setup(x => x.ValidateCreateAsync(dto)).ReturnsAsync(Result.Success);

            // والد پیدا می‌شود
            _mockRepo.Setup(x => x.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentComment);

            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();

            // بررسی ست شدن ParentId
            _mockRepo.Verify(x => x.AddAsync(It.Is<Comment>(c =>
                c.ParentId == parentId
            )), Times.Once);
        }

        // --- Helper ---
        private void ForceSetId(object obj, long id)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) { field.SetValue(obj, id); return; }

                var prop = type.GetProperty("Id");
                if (prop != null && prop.CanWrite) { prop.SetValue(obj, id); return; }

                type = type.BaseType;
            }
        }
    }
}
