using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.OrderContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.OrderServiceApp.Command
{
    public record GetOpenOrderForUserQuery(int userId):IRequest<ErrorOr<OrderUserPanelQueryModel>>;
    public class GetOpenOrderForUserQueryHandler : IRequestHandler<GetOpenOrderForUserQuery, ErrorOr<OrderUserPanelQueryModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductRepository _productRepository;

 
        private readonly ICityRepo _cityRepository;
        // ---

        public GetOpenOrderForUserQueryHandler(
            IOrderRepository orderRepository,
            ISellerRepository sellerRepository,
            IProductSellRepository productSellRepository,
            IProductRepository productRepository,
 
            ICityRepo cityRepository) // تزریق وابستگی جدید
        {
            _orderRepository = orderRepository;
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
            _productRepository = productRepository;
            
            _cityRepository = cityRepository;
        }

        public async Task<ErrorOr<OrderUserPanelQueryModel>> Handle(GetOpenOrderForUserQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش (مشابه هر دو کد)
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.userId);
            if (order == null)
                return Error.NotFound("Order.NotFound", "سفارش باز برای کاربر پیدا نشد.");

            // 2. مپینگ کامل (بر اساس کد دوم)
            OrderUserPanelQueryModel model = new()
            {
                OrderAddress = null, // (بعداً پر می‌شود)
                OrderAddressId = order.OrderAddressId,
                PriceAfterOff = order.PriceAfterOff,
                DiscountId = order.DiscountId,
                DiscountPercent = order.DiscountPercent,
                OrderId = order.Id,
                OrderPayment = order.OrderPayment,
                DiscountTitle = order.DiscountTitle, // (فیلد اصلاح شده از کد دوم)
                Ordersellers = order.OrderSellers.Select(s => new OrderSellerUserPanelQueryModel
                {
                    PriceAfterOff = s.PriceAfterOff,
                    DiscountId = s.DiscountId,
                    DiscountPercent = s.DiscountPercent,
                    Id = s.Id,
                    PostId = s.PostId, // (فیلد اصلاح شده از کد دوم)
                    PostTitle = s.PostTitle, // (فیلد اصلاح شده از کد دوم)
                    DiscountTitle = s.DiscountTitle,
                    OrderItems = s.OrderItems.Select(i => new OrderItemUserPanelQueryModel
                    {
                        PriceAfterOff = i.PriceAfterOff,
                        SumPriceAfterOff = i.SumPriceAfterOff,
                        Count = i.Count,
                        Id = i.Id,
                        Price = i.Price,
                        ProductId = 0,
                        ProductSellId = i.ProductSellId,
                        ProductTitle = "",
                        SumPrice = i.SumPrice,
                        ProductImageAddress = FileDirectories.ProductImageDirectory500,
                        Unit = i.Unit // (فیلد اصلاح شده از کد دوم)
                    }).ToList(),
                    PaymentPrice = s.PaymentPrice,
                    PostPrice = s.PostPrice,
                    Price = s.Price,
                    SellerId = s.SellerId,
                    SellerTitle = "",
                    DiscountPrice = s.DiscountPercent * s.PriceAfterOff / 100 // (فیلد اصلاح شده از کد دوم)
                }).ToList(),
                PaymentPrice = order.PaymentPrice,
                PaymentPriceSeller = order.PaymentPriceSeller,
                PostPrice = order.PostPrice,
                Price = order.Price,
                DiscountPrice = order.DiscountPercent * order.PaymentPriceSeller / 100
            };

            // 3. پر کردن اطلاعات فروشنده و محصولات (مشکل N+1 در هر دو کد وجود دارد)
            foreach (var sellerModel in model.Ordersellers)
            {
                var seller = await _sellerRepository.GetByIdAsync(sellerModel.SellerId);
                sellerModel.SellerTitle = seller?.Title ?? "";

                foreach (var item in sellerModel.OrderItems)
                {
                    var sell = await _productSellRepository.GetByIdAsync(item.ProductSellId);
                    if (sell != null)
                    {
                        var product = await _productRepository.GetByIdAsync(sell.ProductId);
                        if (product != null)
                        {
                            item.ProductId = product.Id;
                            item.ProductTitle = product.Title;
                            item.ProductImageAddress = item.ProductImageAddress + product.ImageName;
                        }
                    }
                }
            }

            // 4. --- اضافه کردن منطق آدرس (جدید از کد دوم) ---
            if (model.OrderAddressId > 0)
            {
                // استفاده از ریپازیتوری‌های جدید به جای DbContext مستقیم
                var address = await _orderRepository.GetOrderAddressByIdAsync(model.OrderAddressId);
                if (address != null)
                {
                    var city = await _cityRepository.GetCityWithStateByIdAsync(address.CityId);
                    if (city != null && city.State != null)
                    {
                        model.OrderAddress = new OrderAddressForOrderUserPanelQueryModel
                        {
                            AddressDetail = address.AddressDetail,
                            CityId = address.CityId,
                            CityName = city.Title,
                            FullName = address.FullName,
                            Id = address.Id,
                            IranCode = address.IranCode,
                            Phone = address.Phone,
                            PostalCode = address.PostalCode,
                            StateId = address.StateId,
                            StateName = city.State.Title
                        };
                    }
                }
            }
            // --- پایان منطق آدرس ---

            return model;
        }
    }
}

