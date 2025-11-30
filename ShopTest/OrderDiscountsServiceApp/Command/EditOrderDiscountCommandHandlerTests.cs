using ErrorOr;
using FluentAssertions;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Moq;
using System.Linq.Expressions;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace ShopTest.OrderDiscountsServiceApp.Command
{
    public class EditOrderDiscountCommandHandlerTests
    {
        private readonly Mock<IOrderDiscountRepository> _mockRepo;
        private readonly EditOrderDiscountCommandHandler _handler;

        public EditOrderDiscountCommandHandlerTests()
        {
            _mockRepo = new Mock<IOrderDiscountRepository>();
            _handler = new EditOrderDiscountCommandHandler(_mockRepo.Object);
        }

        // ----------------------------------------------------------------
        // متدهای کمکی (Helpers)
        // ----------------------------------------------------------------

        private EditOrderDiscount CreateValidEditDto(int id)
        {
            return new EditOrderDiscount
            {
                Id = id,
                Title = "Edited Title",
                Code = "EDITED_CODE",
                Percent = 40,
                Count = 20,
                // تاریخ‌ها باید معتبر باشند (آینده)
                StartDate = DateTime.Now.ToPersianDate(),
                EndDate = DateTime.Now.AddMonths(1).ToPersianDate()
            };
        }

        private OrderDiscount CreateExistingDiscount(int id)
        {
            var discount = new OrderDiscount(
                percent: 10,
                title: "Old Title",
                code: "OLD_CODE",
                count: 5,
                type: OrderDiscountType.Order,
                startDate: DateTime.Now,
                endDate: DateTime.Now.AddDays(10),
                shopId: 1
            );

            // ست کردن ID با Reflection (چون setter پرایوت است)
            ForceSetProperty(discount, "Id", id);
            return discount;
        }

        private void ForceSetProperty(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var prop = type.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.CanWrite) { prop.SetValue(obj, value); return; }

                var field = type.GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) { field.SetValue(obj, value); return; }

                type = type.BaseType;
            }
        }

        // ----------------------------------------------------------------
        // تست‌های Failure
        // ----------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_DiscountIdDoesNotExist()
        {
            // Arrange
            var dto = CreateValidEditDto(999);
            var command = new EditOrderDiscountCommand(dto);

            // ریپازیتوری نال برمی‌گرداند
            _mockRepo.Setup(x => x.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDiscount?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_EndDateIsInThePast()
        {
            // Arrange
            var id = 10;
            var dto = CreateValidEditDto(id);
            dto.EndDate = DateTime.Now.AddDays(-1).ToPersianDate(); // تاریخ گذشته

            var existingDiscount = CreateExistingDiscount(id);
            var command = new EditOrderDiscountCommand(dto);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingDiscount);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be(nameof(EditOrderDiscountCommand));
            result.FirstError.Description.Should().Contain("تاریخ پایان باید حداقل امروز باشد");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public async Task Handle_Should_ReturnValidationError_When_PercentIsInvalid(int invalidPercent)
        {
            // Arrange
            var id = 10;
            var dto = CreateValidEditDto(id);
            dto.Percent = invalidPercent;

            var existingDiscount = CreateExistingDiscount(id);
            var command = new EditOrderDiscountCommand(dto);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingDiscount);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Description.Should().Contain("درصد تخفیف باید بین 1 تا 99 باشد");
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_CodeIsDuplicate()
        {
            // Arrange
            var id = 10;
            var dto = CreateValidEditDto(id);
            dto.Code = "DUPLICATE_CODE";

            var existingDiscount = CreateExistingDiscount(id);
            var command = new EditOrderDiscountCommand(dto);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingDiscount);

            // شبیه‌سازی: کد تکراری است (و متعلق به این ID نیست)
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Description.Should().Contain("کد تخفیف تکراری است");

            // نباید ذخیره شود
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_SaveFails()
        {
            // Arrange
            var id = 10;
            var dto = CreateValidEditDto(id);
            var existingDiscount = CreateExistingDiscount(id);
            var command = new EditOrderDiscountCommand(dto);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingDiscount);
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // ذخیره شکست می‌خورد
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.Failure);
        }

        // ----------------------------------------------------------------
        // تست‌های Success
        // ----------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_UpdateDiscount_And_ReturnSuccess()
        {
            // Arrange
            var id = 10;
            var dto = CreateValidEditDto(id);
            // تغییرات مورد انتظار
            dto.Title = "New Title";
            dto.Percent = 88;

            var existingDiscount = CreateExistingDiscount(id);
            var command = new EditOrderDiscountCommand(dto);

            _mockRepo.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingDiscount);
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // 1. بررسی فراخوانی ذخیره
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. بررسی تغییرات روی موجودیت (State Verification)
            // چون متد Edit روی آبجکت existingDiscount صدا زده شده، مقادیر آن باید تغییر کرده باشد
            existingDiscount.Title.Should().Be("New Title");
            existingDiscount.Percent.Should().Be(88);
        }
    }
}

