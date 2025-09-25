using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Query
{
    public record HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery(int userId, int id):IRequest<ErrorOr<int>>;
    public class HaveUserOpenOrderSellerAsyncByOrderSellerIdQueryHandler : IRequestHandler<HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery, ErrorOr<int>>
    {
        private readonly IOrderRepository _orderRepository;
        public HaveUserOpenOrderSellerAsyncByOrderSellerIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<ErrorOr<int>> Handle(HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            var orderSellerId = request.id;
            var order = await _orderRepository.HaveUserOpenOrderSellerAsyncByOrderSellerIdAsync(userId, orderSellerId);
            if (order.Value == null)
                return Error.NotFound(
                    code: nameof(HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery),
                    description: "سفارشی یافت نشد."
                );
            return order.Value;
        }
    }
}
