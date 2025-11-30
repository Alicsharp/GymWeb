using ErrorOr;
using FluentAssertions;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Application.OrderServiceApp;
using Gtm.Domain.ShopDomain.OrderDomain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace ShopTest.OrderDiscountsServiceApp.Command
{
    public class AddOrderDiscountCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IOrderDiscountValidator> _mockValidator;
        private readonly AddOrderDiscountCommandHandler _handler;
        private readonly AddOrderDiscountCommand _command;
        private const int UserId = 10;
        private const int DiscountId = 200;

        public AddOrderDiscountCommandHandlerTests()
        {
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockValidator = new Mock<IOrderDiscountValidator>();
            _handler = new AddOrderDiscountCommandHandler(_mockOrderRepo.Object, _mockValidator.Object);

            _command = new AddOrderDiscountCommand(UserId, DiscountId, "OFF50", 50);
        }

        // ----------------------------------------------------------------------------------
        // تست مسیر 1: شکست در اعتبارسنجی (نقش ولیدیتور)
        // ----------------------------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_DiscountValidationFails()
        {
            // Arrange
            var validationError = Error.Validation("Discount.Expired", "کد منقضی شده است");

            _mockValidator.Setup(v => v.ValidateCanApplyAsync(DiscountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationError);

            // Act
            var result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Discount.Expired");

            _mockOrderRepo.Verify(x => x.GetOpenOrderForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ----------------------------------------------------------------------------------
        // تست مسیر 2: سفارش باز یافت نشود (Null Check/Guard Clause)
        // ----------------------------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_ReturnNotFound_When_OpenOrderDoesNotExist()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidateCanApplyAsync(DiscountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            // ریپازیتوری نال برمی‌گرداند (استفاده از Task.FromResult برای شبیه‌سازی null در Task)
            _mockOrderRepo.Setup(x => x.GetOpenOrderForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((Order?)null));

            // Act
            var result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.NotFound);
            _mockOrderRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ----------------------------------------------------------------------------------
        // تست مسیر 4: موفقیت (Success Path)
        // ----------------------------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_ApplyDiscountAndVerifyStateChange_And_ReturnSuccess()
        {
            // Arrange
            var order = new Order(UserId); // استفاده از سازنده واقعی کلاس Order

            // ولیدیشن موفق
            _mockValidator.Setup(v => v.ValidateCanApplyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            _mockOrderRepo.Setup(x => x.GetOpenOrderForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockOrderRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // 1. بررسی فراخوانی متدهای دیتابیس
            _mockOrderRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // 2. بررسی State Verification (آیا تخفیف واقعاً روی Order اعمال شده؟)
            // این چک، متد order.AddDiscount را تست می‌کند.
            order.DiscountId.Should().Be(DiscountId);
            order.DiscountPercent.Should().Be(50);
            order.DiscountTitle.Should().Be("OFF50");
        }

        // ----------------------------------------------------------------------------------
        // متدهای کمکی Reflection
        // ----------------------------------------------------------------------------------

        // متد کمکی برای ست کردن پراپرتی‌های Private/Base Class
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
