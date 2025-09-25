using ErrorOr;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query
{
    public record GetAllInActivesForAdminQuery:IRequest<ErrorOr<List<OrderAdminQueryModel>>>;
    public class GetAllInActivesForAdminQueryHandler : IRequestHandler<GetAllInActivesForAdminQuery, ErrorOr<List<OrderAdminQueryModel>>>
    {
        private readonly IOrderDiscountRepository _orderDiscountRepository;

        public GetAllInActivesForAdminQueryHandler(IOrderDiscountRepository orderDiscountRepository)
        {
            _orderDiscountRepository = orderDiscountRepository;
        }
        public async Task<ErrorOr<List<OrderAdminQueryModel>>> Handle(GetAllInActivesForAdminQuery request, CancellationToken cancellationToken)
        {
            var res = _orderDiscountRepository.QueryBy(o => o.EndDate.Date < DateTime.Now.Date && o.Type == OrderDiscountType.Order);
            return res.Select(r => new OrderAdminQueryModel(r.Id, r.Percent, r.Title, r.Code, r.Count, r.StartDate, r.EndDate, r.Use, r.CreateDate)).ToList();
        }
    }
}
