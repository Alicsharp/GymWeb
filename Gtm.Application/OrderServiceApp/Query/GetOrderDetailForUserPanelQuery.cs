using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.OrderServiceApp.Query
{
    /// <summary>
    /// کوئری برای دریافت جزئیات کامل سفارش پرداخت شده برای پنل کاربر
    /// </summary>
    public record GetOrderDetailForUserPanelQuery(int OrderId,int userId)
        : IRequest<ErrorOr<OrderDetailForUserPanelQueryModel>>;
    public class GetOrderDetailForUserPanelQueryHandler
    : IRequestHandler<GetOrderDetailForUserPanelQuery, ErrorOr<OrderDetailForUserPanelQueryModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICityRepo _cityRepository; 
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetOrderDetailForUserPanelQueryHandler(IOrderRepository orderRepository, ICityRepo cityRepository, ISellerRepository sellerRepository, IProductSellRepository productSellRepository)
        {
            _orderRepository = orderRepository;
            _cityRepository = cityRepository;
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<OrderDetailForUserPanelQueryModel>> Handle(GetOrderDetailForUserPanelQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderDetailForUserPanelAsync(request.OrderId, request.userId);
            if (order == null) return Error.NotFound();
            OrderDetailForUserPanelQueryModel model = new()
            {
                OrderAddress = new(),
                PriceAfterOff = order.PriceAfterOff,
                DiscountPercent = order.DiscountPercent,
                DiscountTitle = order.DiscountTitle,
                Id = order.Id,
                OrderPayment = order.OrderPayment,
                OrderSellers = order.OrderSellers.Select(s => new OrderSellerDetailForUserPanelQueryModel
                {
                    PriceAfterOff = s.PriceAfterOff,
                    SellerAddress = "",
                    DiscountId = s.DiscountId,
                    DiscountPercent = s.DiscountPercent,
                    DiscountTitle = s.DiscountTitle,
                    Id = s.Id,
                    DiscountPrice = s.DiscountPercent * s.PriceAfterOff / 100,
                    OrderId = s.OrderId,
                    OrderItems = s.OrderItems.Select(i => new OrderItemDetailForUserPanelQueryModel
                    {
                        PriceAfterOff = i.PriceAfterOff,
                        SumPriceAfterOff = i.SumPriceAfterOff,
                        Count = i.Count,
                        Id = i.Id,
                        OrderSellerId = i.OrderSellerId,
                        Price = i.Price,
                        ProductId = 0,
                        ProductImageName = "",
                        ProductSellId = i.ProductSellId,
                        ProductTitle = "",
                        SumPrice = i.SumPrice,
                        Unit = i.Unit
                    }).ToList(),
                    PaymentPrice = s.PaymentPrice,
                    PostId = s.PostId,
                    PostPrice = s.PostPrice,
                    PostTitle = s.PostTitle,
                    Price = s.Price,
                    SellerId = s.SellerId,
                    Status = s.Status,
                }).ToList(),
                OrderStatus = order.OrderStatus,
                PaymentPrice = order.PaymentPrice,
                PaymentPriceSeller = order.PaymentPriceSeller,
                PostPrice = order.PostPrice,
                Price = order.Price,
                UpdateDate = order.UpdateDate.ToPersianDate()
            };
            var address = await _orderRepository.GetOrderAddressByIdAsync(order.OrderAddressId);
            var city = await _cityRepository.GetCityWithStateAsync(c => c.Id == address.CityId && c.StateId == address.StateId);
            model.OrderAddress = new()
            {
                AddressDetail = address.AddressDetail,
                City = city.Title,
                CityId = address.CityId,
                FullName = address.FullName,
                IranCode = address.IranCode,
                Phone = address.Phone,
                PostalCode = address.PostalCode,
                State = city.State.Title,
                StateId = address.StateId
            };
            foreach (var seller in model.OrderSellers)
            {
                var s = await _sellerRepository.GetByIdAsync(seller.SellerId);
                var c = await _cityRepository.GetCityWithStateAsync(c => c.Id == address.CityId && c.StateId == address.StateId);
                seller.SellerAddress = $"{s.Title} - {c.State.Title} - {c.Title} - {s.Address}";
                foreach (var item in seller.OrderItems)
                {
                    var productSell= await _productSellRepository.GetProductSellWithProductAsync( item.ProductSellId);
                    item.ProductId = productSell.ProductId;
                    item.ProductTitle = productSell.Product.Title;
                    item.ProductImageName = $"{FileDirectories.ProductImageDirectory100}{productSell.Product.ImageName}";
                }
            }
            return model;
        }
    }
}
