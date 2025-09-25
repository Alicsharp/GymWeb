using ErrorOr;
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

        public GetOpenOrderForUserQueryHandler(IOrderRepository orderRepository, ISellerRepository sellerRepository, IProductSellRepository productSellRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _sellerRepository = sellerRepository;
            _productSellRepository = productSellRepository;
            _productRepository = productRepository;
        }

        public async Task<ErrorOr<OrderUserPanelQueryModel>> Handle(GetOpenOrderForUserQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOpenOrderForUserAsync(request.userId);
            if (order == null)
                return Error.NotFound("Order.NotFound", "سفارش باز برای کاربر پیدا نشد.");

            OrderUserPanelQueryModel model = new()
            {
                OrderAddress = null,
                OrderAddressId = order.OrderAddressId,
                PriceAfterOff = order.PriceAfterOff,
                DiscountId = order.DiscountId,
                DiscountPercent = order.DiscountPercent,
                OrderId = order.Id,
                OrderPayment = order.OrderPayment,
                Ordersellers = order.OrderSellers.Select(s => new OrderSellerUserPanelQueryModel
                {
                    PriceAfterOff = s.PriceAfterOff,
                    DiscountId = s.DiscountId,
                    DiscountPercent = s.DiscountPercent,
                    Id = s.Id,
                    DiscountTitle = s.DiscountTitle,
                    OrderItems = s.OrderItems.Select(i => new OrderItemUserPanelQueryModel
                    {
                        PriceAfterOff = i.PriceAfterOff,
                        SumPriceAfterOff = i.SumPriceAfterOff,
                        Count = i.Count,
                        Id = i.Id,
                        Price = i.Price,
                        ProductId = 0, // پر میشه بعدا
                        ProductSellId = i.ProductSellId,
                        ProductTitle = "",
                        SumPrice = i.SumPrice,
                        ProductImageAddress = FileDirectories.ProductImageDirectory500
                    }).ToList(),
                    PaymentPrice = s.PaymentPrice,
                    PostPrice = s.PostPrice,
                    Price = s.Price,
                    SellerId = s.SellerId,
                    SellerTitle = "" // پر میشه بعدا
                }).ToList(),
                PaymentPrice = order.PaymentPrice,
                PaymentPriceSeller = order.PaymentPriceSeller,
                PostId = order.PostId,
                PostPrice = order.PostPrice,
                PostTitle = order.PostTitle,
                Price = order.Price
            };

            // پر کردن اطلاعات فروشنده و محصولات
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

            return model;
        }
    }
  }

