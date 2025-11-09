using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.OrderContract.Query;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using Gtm.Domain.ShopDomain.SellerDomain;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.OrderServiceApp.Query
{
    /// <summary>
    /// کوئری برای دریافت جزئیات یک فروش-سفارش خاص برای پنل فروشنده
    /// (همراه با بررسی امنیتی مالکیت)
    /// </summary>
    public record GetOrderDetailForSellerPanelQuery(int OrderSellerId, int UserId)
        : IRequest<ErrorOr<OrderSellerDetailForSellerPanelQueryModel>>;

    public class GetOrderDetailForSellerPanelQueryHandler
    : IRequestHandler<GetOrderDetailForSellerPanelQuery, ErrorOr<OrderSellerDetailForSellerPanelQueryModel>>
    {
        private readonly IOrderRepository _orderSellerRepository;
        private readonly ISellerRepository _sellerRepository;
  
        private readonly ICityRepo _cityRepository;

        public GetOrderDetailForSellerPanelQueryHandler(IOrderRepository orderSellerRepository, ISellerRepository sellerRepository, ICityRepo cityRepository)
        {
            _orderSellerRepository = orderSellerRepository;
            _sellerRepository = sellerRepository;
            _cityRepository = cityRepository;
        }

        public async Task<ErrorOr<OrderSellerDetailForSellerPanelQueryModel>> Handle(
            GetOrderDetailForSellerPanelQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش (بهینه - شامل Order, Items, ProductSell, Product)
            var orderSeller = await _orderSellerRepository.GetOrderSellerWithDetailsAsync(request.OrderSellerId, cancellationToken);

            if (orderSeller == null || orderSeller.Order == null)
                return Error.NotFound(description: "سفارش یافت نشد.");

            // 2. بررسی امنیتی (آیا این کاربر مالک این فروشنده است؟)
            var isSellerOwner = await _sellerRepository.IsSellerForUserAsync(orderSeller.SellerId, request.UserId, cancellationToken);
            if (!isSellerOwner)
                return Error.Forbidden(description: "شما دسترسی به این سفارش ندارید.");

            // 3. واکشی موازی آدرس و اطلاعات فروشنده (برای بهینگی)
            Task<OrderAddress> addressTask = _orderSellerRepository.GetOrderAddressByIdAsync(orderSeller.Order.OrderAddressId);
            Task<Seller> sellerTask = _sellerRepository.GetByIdAsync(orderSeller.SellerId);

            await Task.WhenAll(addressTask, sellerTask);

            var address = addressTask.Result;
            var seller = sellerTask.Result;

            if (address == null) return Error.NotFound(description: "آدرس سفارش یافت نشد.");
            if (seller == null) return Error.NotFound(description: "اطلاعات فروشنده یافت نشد.");

            // 4. واکشی شهر (وابسته به آدرس)
            var city = await _cityRepository.GetCityWithStateAsync(address.CityId, address.StateId);
            if (city == null || city.State == null) return Error.NotFound(description: "شهر یا استان آدرس یافت نشد.");

            // 5. مپ کردن مدل نهایی (اکنون تمام داده‌ها در حافظه هستند)
            OrderSellerDetailForSellerPanelQueryModel model = new()
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
                CreationDate = orderSeller.Order.CreateDate.ToPersainDate(),
                UpdateDate = orderSeller.Order.UpdateDate.ToPersainDate(),

                // 5a. مپ کردن آدرس
                OrderAddress = new() // (فرض بر وجود این DTO)
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
                },

                // 5b. مپ کردن آدرس فروشنده
                SellerAddress = seller.Title, // (طبق منطق کد اصلی شما)

                // 5c. مپ کردن آیتم‌ها (حل مشکل N+1)
                OrderItems = orderSeller.OrderItems.Select(i => new OrderItemDetailForSellerPanelQueryModel
                {
                    PriceAfterOff = i.PriceAfterOff,
                    SumPriceAfterOff = i.SumPriceAfterOff,
                    Count = i.Count,
                    Id = i.Id,
                    OrderSellerId = i.OrderSellerId,
                    Price = i.Price,
                    ProductSellId = i.ProductSellId,
                    Unit = i.Unit,
                    SumPrice = i.SumPrice,
                    // (داده‌های محصول از قبل Include شده‌اند)
                    ProductId = i.ProductSell?.ProductId ?? 0,
                    ProductTitle = i.ProductSell?.Product?.Title ?? "",
                    ProductImageName = (i.ProductSell?.Product != null)
                        ? $"{FileDirectories.ProductImageDirectory100}{i.ProductSell.Product.ImageName}"
                        : ""
                }).ToList()
            };

            return model;
        }
    }
}
