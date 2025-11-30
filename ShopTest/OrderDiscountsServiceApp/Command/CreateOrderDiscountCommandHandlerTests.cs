using ErrorOr;
using FluentAssertions;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using Gtm.Domain.DiscountsDomain.OrderDiscount;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace ShopTest.OrderDiscountsServiceApp.Command
{
    // ✅ کلاس DTO کمکی با نام "CreateOrderDiscount" (مطابق با کامند)
    public class CreateOrderDiscountCommandHandlerTests
    {
        private readonly Mock<IOrderDiscountRepository> _mockRepo;
        private readonly CreateOrderDiscountCommandHandler _handler;

        public CreateOrderDiscountCommandHandlerTests()
        {
            _mockRepo = new Mock<IOrderDiscountRepository>();
            _handler = new CreateOrderDiscountCommandHandler(_mockRepo.Object);
        }

        // ----------------------------------------------------------------
        // متد کمکی برای ساخت DTO معتبر (از نوع کلاس اصلی)
        // ----------------------------------------------------------------
        private CreateOrderDiscount CreateValidDto()
        {
            return new CreateOrderDiscount
            {
                Title = "تخفیف تابستانه",
                Code = "SUMMER2025",
                Percent = 50,
                Count = 100,
                // تاریخ‌ها را طوری تنظیم می‌کنیم که از ولیدیشن‌های هندلر عبور کنند
                StartDate = DateTime.Now.ToPersianDate(),
                EndDate = DateTime.Now.AddMonths(1).ToPersianDate()
            };
        }

        // ----------------------------------------------------------------
        // تست‌های Failure (خطاهای منطقی و بیزینسی هندلر)
        // ----------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_EndDateIsInThePast()
        {
            // Arrange
            var dto = CreateValidDto();
            dto.EndDate = DateTime.Now.AddDays(-1).ToPersianDate(); // تاریخ پایان: دیروز

            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Date.Invalid");
            result.FirstError.Description.Should().Contain("تاریخ پایان باید حداقل امروز باشد");
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_EndDateIsBeforeStartDate()
        {
            // Arrange
            var dto = CreateValidDto();
            dto.StartDate = DateTime.Now.AddDays(5).ToPersianDate();
            dto.EndDate = DateTime.Now.AddDays(1).ToPersianDate(); // پایان قبل از شروع است

            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Date.Invalid");
            result.FirstError.Description.Should().Contain("نمی‌تواند قبل از تاریخ شروع باشد");
        }

        [Theory]
        [InlineData(0)]   // کمتر از 1
        [InlineData(100)] // بیشتر از 99
        public async Task Handle_Should_ReturnValidationError_When_PercentIsInvalid(int invalidPercent)
        {
            // Arrange
            var dto = CreateValidDto();
            dto.Percent = invalidPercent;

            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Percent.Invalid");
        }

        [Fact]
        public async Task Handle_Should_ReturnValidationError_When_CountIsZero()
        {
            // Arrange
            var dto = CreateValidDto();
            dto.Count = 0; // نامعتبر

            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Count.Invalid");
        }

        [Fact]
        public async Task Handle_Should_ReturnConflict_When_DiscountCodeExists()
        {
            // Arrange
            var dto = CreateValidDto();
            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // شبیه‌سازی: ریپازیتوری می‌گوید این کد تکراری است
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Type.Should().Be(ErrorType.Conflict);
            result.FirstError.Code.Should().Be("Discount.Duplicate");

            // مطمئن می‌شویم که متد AddAsync صدا زده نشده است
            _mockRepo.Verify(x => x.AddAsync(It.IsAny<OrderDiscount>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_DatabaseSaveFails()
        {
            // Arrange
            var dto = CreateValidDto();
            var command = new CreateOrderDiscountCommand(dto, OrderDiscountType.Order, 1);

            // کد تکراری نیست
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // اما ذخیره در دیتابیس شکست می‌خورد (مثلاً مشکل کانکشن)
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Code.Should().Be("Database.SaveError");
        }

        // ----------------------------------------------------------------
        // تست موفقیت (Happy Path)
        // ----------------------------------------------------------------

        [Fact]
        public async Task Handle_Should_CreateDiscount_And_ReturnSuccess()
        {
            // Arrange
            var dto = CreateValidDto();
            var shopId = 100;
            var discountType = OrderDiscountType.OrderSeller; // تست با نوع متفاوت

            var command = new CreateOrderDiscountCommand(dto, discountType, shopId);

            // 1. کد تکراری نیست
            _mockRepo.Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<OrderDiscount, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // 2. ذخیره موفقیت‌آمیز است
            _mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // الف: نتیجه باید موفق باشد
            result.IsError.Should().BeFalse();
            result.Value.Should().Be(Result.Success);

            // ب: بررسی اینکه آیا متد AddAsync با داده‌های درست صدا زده شده است؟
            _mockRepo.Verify(x => x.AddAsync(It.Is<OrderDiscount>(d =>
                d.Code == dto.Code.Trim() &&
                d.ShopId == shopId &&
                d.Type == discountType &&
                d.Percent == dto.Percent &&
                d.Count == dto.Count
            // می‌توان تاریخ‌ها را هم چک کرد اما چون تبدیل تاریخ داریم کمی پیچیده می‌شود
            )), Times.Once);

            // ج: بررسی اینکه SaveChangesAsync دقیقاً یک بار صدا زده شده
            _mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

