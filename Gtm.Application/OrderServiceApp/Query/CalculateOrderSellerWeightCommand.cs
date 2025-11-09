using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Query
{
    public record GetOrderSellerWeightQuery(int OrderSellerId) : IRequest<ErrorOr<int>>;
    public class GetOrderSellerWeightQueryHandler : IRequestHandler<GetOrderSellerWeightQuery, ErrorOr<int>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderSellerWeightQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ErrorOr<int>> Handle(GetOrderSellerWeightQuery request, CancellationToken cancellationToken)
        {
            // ریپازیتوری حالا یا وزن را برمی‌گرداند یا خطا
            var result = await _orderRepository.CalculateOrderSellerWeightAsync(request.OrderSellerId);

            // نتیجه (چه موفقیت چه خطا) را مستقیماً برمی‌گردانیم
            return result;
        }
    }
}
