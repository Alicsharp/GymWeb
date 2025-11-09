using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.OrderContract.Query;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetOrderSellerDetailForSellerPanelQuery(int OrderSellerId, int UserId)
    : IRequest<ErrorOr<OrderSellerDetailForSellerPanelQueryModel>>;
    public class GetOrderSellerDetailForSellerPanelQueryHandler
    : IRequestHandler<GetOrderSellerDetailForSellerPanelQuery, ErrorOr<OrderSellerDetailForSellerPanelQueryModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IPostRepo _postRepository;
        private readonly IProductSellRepository _productSellRepository;
 
        public GetOrderSellerDetailForSellerPanelQueryHandler(
            IOrderRepository orderRepository,
            ISellerRepository sellerRepository,
            IPostRepo postRepository,
            IProductSellRepository productSellRepository)
        {
            _orderRepository = orderRepository;
            _sellerRepository = sellerRepository;
            _postRepository = postRepository;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<OrderSellerDetailForSellerPanelQueryModel>> Handle(
            GetOrderSellerDetailForSellerPanelQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ واکشی OrderSeller به همراه Order و OrderItems
            var orderSeller = await _orderRepository.GetOrderSellerDetailForSellerPanelAsync(request.OrderSellerId, cancellationToken);
            if (orderSeller == null)
                return Error.NotFound("OrderSeller.NotFound", "فروشنده‌ای با این شناسه یافت نشد.");

            // 2️⃣ بررسی مالکیت فروشنده
            var seller = await _sellerRepository.GetByIdAsync(orderSeller.SellerId);
            if (seller == null || seller.UserId != request.UserId)
                return Error.Validation("OrderSeller.AccessDenied", "شما به این سفارش دسترسی ندارید.");

            // 3️⃣ ساخت مدل خروجی
            var model = new OrderSellerDetailForSellerPanelQueryModel
            {
                PriceAfterOff = orderSeller.PriceAfterOff,
                DiscountId = orderSeller.DiscountId,
                DiscountPercent = orderSeller.DiscountPercent,
                DiscountTitle = orderSeller.DiscountTitle,
                UserCustomerId = orderSeller.Order.UserId,
                Id = orderSeller.Id,
                DiscountPrice = orderSeller.DiscountPercent * orderSeller.PriceAfterOff / 100,
                OrderId = orderSeller.OrderId,
                PaymentPrice = orderSeller.PaymentPrice,
                PostId = orderSeller.PostId,
                PostPrice = orderSeller.PostPrice,
                PostTitle = orderSeller.PostTitle,
                Price = orderSeller.Price,
                SellerId = orderSeller.SellerId,
                Status = orderSeller.Status,
                OrderAddress = null,
                CreationDate = orderSeller.Order.CreateDate.ToPersainDate(),
                UpdateDate = orderSeller.Order.UpdateDate.ToPersainDate()
            };

            // 4️⃣ آدرس سفارش
            var address = await _orderRepository.GetOrderAddressWithCityAndStateAsync(orderSeller.Order.OrderAddressId);
            

            // 5️⃣ آدرس فروشنده
            model.SellerAddress = seller.Title;

            // 6️⃣ پر کردن اطلاعات محصولات
            model.OrderItems = new List<OrderItemDetailForSellerPanelQueryModel>();
            foreach (var item in orderSeller.OrderItems)
            {
                var productSell = await _productSellRepository.GetWithProductAsync(item.ProductSellId, cancellationToken);
                model.OrderItems.Add(new OrderItemDetailForSellerPanelQueryModel
                {
                    PriceAfterOff = item.PriceAfterOff,
                    SumPriceAfterOff = item.SumPriceAfterOff,
                    Count = item.Count,
                    Id = item.Id,
                    OrderSellerId = item.OrderSellerId,
                    Price = item.Price,
                    ProductId = productSell.ProductId,
                    ProductSellId = item.ProductSellId,
                    ProductTitle = productSell.Product.Title,
                    ProductImageName = $"{FileDirectories.ProductImageDirectory100}{productSell.Product.ImageName}",
                    SumPrice = item.SumPrice,
                    Unit = item.Unit
                });
            }

            return model;
        }
    }

}
