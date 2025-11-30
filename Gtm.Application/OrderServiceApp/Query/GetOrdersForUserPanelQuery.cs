using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.OrderServiceApp.Query
{
    /// <summary>
    /// کوئری برای دریافت لیست سفارش‌های کاربر به صورت صفحه‌بندی شده
    /// </summary>
    public record GetOrdersForUserPanelQuery(int UserId, int PageId, int Take)
        : IRequest<OrderUserPanelPaging>;

    public class GetOrdersForUserPanelQueryHandler
    : IRequestHandler<GetOrdersForUserPanelQuery, OrderUserPanelPaging>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersForUserPanelQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderUserPanelPaging> Handle(GetOrdersForUserPanelQuery request, CancellationToken cancellationToken)
        {
            var res =   _orderRepository.GetUserOrdersQueryable(request.UserId);
            OrderUserPanelPaging model = new();
            model.GetData(res, request.PageId,request.Take, 2);
            model.Orders = new();
            if (res.Count() > 0)
                model.Orders = res.Skip(model.Skip).Take(model.Take)
                    .Select(o => new OrderForUserPanelQueryModel
                    {
                        PriceAfterOff = o.PriceAfterOff,
                        CreationDate = o.CreateDate.ToPersianDate(),
                        DiscountPercent = o.DiscountPercent,
                        DiscountTitle = o.DiscountTitle,
                        Id = o.Id,
                        OrderStatus = o.OrderStatus,
                        PaymentPrice = o.PaymentPrice,
                        PaymentPriceSeller = o.PaymentPriceSeller,
                        PostPrice = o.PostPrice,
                        Price = o.Price,
                        UpdateDate = o.UpdateDate.ToPersianDate()
                    }).ToList();
            return model;

        }
    }}
