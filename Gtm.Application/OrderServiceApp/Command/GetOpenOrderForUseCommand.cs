using Gtm.Domain.ShopDomain.OrderDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record GetOrCreateOpenOrderCommand(int UserId) : IRequest<Order>;
    public class GetOrCreateOpenOrderCommandHandler : IRequestHandler<GetOrCreateOpenOrderCommand, Order>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrCreateOpenOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Order> Handle(GetOrCreateOpenOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. ابتدا سعی می‌کنیم سفارش را واکشی کنیم
            var order = await _orderRepository.GetOpenOrderWithItemsAsync(request.UserId);

            // 2. اگر وجود نداشت، آن را ایجاد می‌کنیم
            if (order == null)
            {
                order = new(request.UserId); // ساخت انتیتی جدید
               await _orderRepository.AddAsync(order); // افزودن به Context
                await _orderRepository.SaveChangesAsync(); // ذخیره در دیتابیس

                // نیازی به واکشی مجدد نیست. آبجکت 'order'
                // همان آبجکت ایجاد شده است.
            }

            // 3. سفارش (چه قدیمی چه جدید) را برمی‌گردانیم
            return order;
        }
    }
}
