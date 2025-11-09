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
    public record CreateOrUpdateOrderAddressCommand(CreateOrderAddress Model, int UserId): IRequest<ErrorOr<Success>>;
    public class CreateOrUpdateOrderAddressCommandHandler: IRequestHandler<CreateOrUpdateOrderAddressCommand, ErrorOr<Success>>
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrUpdateOrderAddressCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateOrUpdateOrderAddressCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            var userId = request.UserId;

            // 1. واکشی سفارش باز
            var order = await _orderRepository.GetOpenOrderForUserAsync(userId);

            // (این بررسی حیاتی در کد اصلی شما وجود نداشت و می‌توانست باعث خطا شود)
            if (order == null)
            {
                return Error.NotFound(description: "سبد خرید بازی یافت نشد.");
            }

            // 2. بررسی برای ویرایش یا ایجاد
            OrderAddress address = null;
            if (order.OrderAddressId > 0)
            {
                // اگر سفارش آدرس داشت، سعی در واکشی آن می‌کنیم
                address = await _orderRepository.GetOrderAddressByIdAsync(order.OrderAddressId);
            }

            // 3. اجرای منطق
            if (address != null)
            {
                // --- مسیر ویرایش ---
                // (در کد اصلی شما، model.Phone دوبار پاس داده شده بود، من آن را حفظ کردم)
                address.Edit(model.StateId, model.CityId, model.AddressDetail, model.PostalCode, model.Phone, model.Phone, model.IranCode);
            }
            else
            {
                // --- مسیر ایجاد ---
                // (این حالت هم زمانی که OrderAddressId صفر است و هم زمانی که ID دارد 
                // اما آدرس در دیتابیس null است، اجرا می‌شود)

                OrderAddress orderAddress = new(model.StateId, model.CityId, model.AddressDetail, model.PostalCode, model.Phone, model.FullName, model.IranCode, order.Id);

                var key = await _orderRepository.CreateOrderaddressReturnKey(orderAddress);

                if (key > 0)
                {
                    order.ChangeAddress(key);
                }
                else
                {
                    // اگر ایجاد آدرس در دیتابیس شکست بخورد
                    return Error.Failure(description: ValidationMessages.SystemErrorMessage);
                }
            }

            // 4. منطق مشترک (که در کد اصلی شما وجود داشت)
            foreach (var item in order.OrderSellers)
            {
                item.AddPostPrice(0, 0, "");
            }

            // 5. ذخیره نهایی
            if (await _orderRepository.SaveChangesAsync())
            {
                // به جای: return new(true);
                return Result.Success;
            }

            // به جای: return new(false, ValidationMessages.SystemErrorMessage);
            return Error.Failure(description: ValidationMessages.SystemErrorMessage);
        }
    }
}
