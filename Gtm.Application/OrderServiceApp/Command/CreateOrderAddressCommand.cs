using ErrorOr;
using Gtm.Contract.OrderAddressContract.Command;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record CreateOrderAddressCommand(CreateOrderAddress Model, int UserId): IRequest<ErrorOr<Success>>;
    public partial class CreateOrderAddressCommandHandler: IRequestHandler<CreateOrderAddressCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;
        // (فرض می‌کنیم ValidationMessages یک کلاس static با پیام‌های خطا است)

        public CreateOrderAddressCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateOrderAddressCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var userId = request.UserId;

            // 1. واکشی سفارش باز
            var order = await _orderRepository.GetOpenOrderForUserAsync(userId);

            if (order == null)
            {
                return Error.NotFound(description: "سبد خرید بازی یافت نشد.");
            }

            // 2. بررسی کلیدی: این کامند "فقط ایجاد" است
            // اگر سفارش از قبل آدرس داشته باشد، نباید آن را ویرایش کند، بلکه باید خطا دهد.
            if (order.OrderAddressId > 0)
            {
                return Error.Validation(description: "این سفارش از قبل دارای آدرس است.");
                // (اگر می‌خواستید حالت "ایجاد" حتی اگر آدرس قبلی null بود را هندل کنید،
                // باید کد قبلی (GetOrderAddressByIdAsync) را هم اضافه می‌کردیم،
                // اما این پیاده‌سازی تمیزتر است)
            }

            // 3. --- مسیر ایجاد ---
            OrderAddress orderAddress = new(
                model.StateId,
                model.CityId,
                model.AddressDetail,
                model.PostalCode,
                model.Phone,
                model.FullName,
                model.IranCode,
                order.Id);

            var key = await _orderRepository.CreateOrderaddressReturnKey(orderAddress);

            if (key <= 0)
            {
                // اگر ایجاد آدرس در دیتابیس شکست بخورد
                return Error.Failure(description: ValidationMessages.SystemErrorMessage);
            }

            // 4. اتصال آدرس جدید به سفارش
            order.ChangeAddress(key); // (فرض می‌کنیم متد ChangeAddress در انتیتی Order دارید)

            // 5. ریست کردن هزینه پستی
            foreach (var item in order.OrderSellers)
            {
                item.AddPostPrice(0, 0, "");
            }

            // 6. ذخیره نهایی تغییرات در سفارش
            if (await _orderRepository.SaveChangesAsync())
            {
                return Result.Success;
            }

            return Error.Failure(description: ValidationMessages.SystemErrorMessage);
        }
    }
}
