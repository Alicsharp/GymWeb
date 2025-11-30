using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.OrderContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.OrderServiceApp.Query
{
    public record GetPaidOrderDetailQuery(int OrderId)
       : IRequest<ErrorOr<OrderDetailForUserPanelQueryModel>>;
    public class GetPaidOrderDetailQueryHandler
    : IRequestHandler<GetPaidOrderDetailQuery, ErrorOr<OrderDetailForUserPanelQueryModel>>
    {
        // برای کوئری اول
        private readonly IOrderRepository _orderRepository;
        private readonly ICityRepo _cityRepo;
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductSellRepository _productSellRepository;

        public GetPaidOrderDetailQueryHandler(IOrderRepository orderRepository, ICityRepo cityRepo, ISellerRepository sellerRepository, IProductSellRepository productSellRepository)
        {
            _orderRepository = orderRepository;
            _cityRepo = cityRepo;
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<OrderDetailForUserPanelQueryModel>> Handle(
            GetPaidOrderDetailQuery request, CancellationToken cancellationToken)
        {
            // 1. استفاده از متد ریپازیتوری (طبق درخواست شما)
            var order = await _orderRepository.GetOrderWithSellersAndItemsAsync(request.OrderId);

            // 2. اعتبارسنجی (از کد اصلی)
            if (order == null || order.OrderStatus !=  OrderStatus.پرداخت_شده)
            {
                return Error.NotFound("سفارش پرداخت شده‌ای یافت نشد.");
            }

            // 3. مپ کردن اولیه (دقیقاً مانند کد اصلی)
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

            // 4. واکشی آدرس (N+1) (حفظ ساختار اصلی اما به صورت async)
            var address = await _orderRepository.GetOrderAddressByIdAsync(order.OrderAddressId);
              
            if (address == null) return Error.NotFound("آدرس سفارش یافت نشد.");

            var city = await _cityRepo.GetCityWithStateAsync(c => c.Id == address.CityId && c.StateId == address.StateId);
                
            if (city == null || city.State == null) return Error.NotFound("شهر یا استان آدرس یافت نشد.");

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

            // 5. حلقه‌های N+1 (حفظ ساختار اصلی اما به صورت async)
            foreach (var seller in model.OrderSellers)
            {
                var s = await _sellerRepository.GetByIdAsync(seller.SellerId);
                if (s == null) continue;

                var c = await   _cityRepo.GetCityWithStateAsync(c => c.Id == address.CityId && c.StateId == address.StateId);
                if (c == null || c.State == null) continue;

                seller.SellerAddress = $"{s.Title} - {c.State.Title} - {c.Title} - {s.Address}";

                foreach (var item in seller.OrderItems)
                {
                    var productSell = await _productSellRepository.GetProductSellWithProductAsync(item.ProductId);
                    if (productSell == null || productSell.Product == null) continue;

                    item.ProductId = productSell.ProductId;
                    item.ProductTitle = productSell.Product.Title;
                    item.ProductImageName = $"{FileDirectories.ProductImageDirectory100}{productSell.Product.ImageName}";
                }
            }

            return model;
        }
    }
}
