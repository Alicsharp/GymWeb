using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.UserApp;
using Gtm.Contract.AdminDashboard;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.AdminDashbord.Query
{
    public record GetOrderDetailForAdminQuery(int Id)
    : IRequest<ErrorOr<OrderDetailForAdminQueryModel>>;
    public class GetOrderDetailForAdminQueryHandler
    : IRequestHandler<GetOrderDetailForAdminQuery, ErrorOr<OrderDetailForAdminQueryModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepo _userRepository;
        private readonly ICityRepo _cityRepo;
        private readonly IProductSellRepository  _productSellRepository;
     
        public GetOrderDetailForAdminQueryHandler(
            IOrderRepository orderRepository,
            IUserRepo userRepository,
            ICityRepo cityRepo,
            IProductSellRepository productSellRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _cityRepo = cityRepo;
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<OrderDetailForAdminQueryModel>> Handle(
            GetOrderDetailForAdminQuery request,
            CancellationToken cancellationToken)
        {
            // 1. واکشی سفارش
            var order = await _orderRepository.GetOrderDetailForAdminAsync(request.Id, cancellationToken);
            if (order == null)
                return Error.NotFound(description: "سفارش یافت نشد");

            // 2. ایجاد مدل خروجی
            var model = new OrderDetailForAdminQueryModel
            {
                Id = order.Id,
                PriceAfterOff = order.PriceAfterOff,
                DiscountPercent = order.DiscountPercent,
                DiscountTitle = order.DiscountTitle,
                OrderPayment = order.OrderPayment,
                OrderStatus = order.OrderStatus,
                PaymentPrice = order.PaymentPrice,
                PaymentPriceSeller = order.PaymentPriceSeller,
                PostPrice = order.PostPrice,
                Price = order.Price,
                UpdateDate = order.UpdateDate.ToPersianDate(),
                User = new(),
                OrderAddress = null
            };

            // 3. آدرس سفارش
            if (order.OrderAddressId > 0)
            {
                var address = order.OrderAddress;
                var city = await _cityRepo.GetCityWithStateAsync(address.CityId, address.StateId);
                if (city != null)
                {
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
                }
            }

            // 4. فروشندگان
            model.OrderSellers = new List<OrderSellerDetailForAdminQueryModel>();
            foreach (var s in order.OrderSellers)
            {
                var seller = s.Seller;
                var city = await _cityRepo.GetCityWithStateAsync(seller.CityId, seller.StateId);

                var sellerModel = new OrderSellerDetailForAdminQueryModel
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    SellerId = s.SellerId,
                    Price = s.Price,
                    PriceAfterOff = s.PriceAfterOff,
                    DiscountId = s.DiscountId,
                    DiscountPercent = s.DiscountPercent,
                    DiscountTitle = s.DiscountTitle,
                    DiscountPrice = s.DiscountPercent * s.PriceAfterOff / 100,
                    PaymentPrice = s.PaymentPrice,
                    PostId = s.PostId,
                    PostPrice = s.PostPrice,
                    PostTitle = s.PostTitle,
                    Status = s.Status,
                    SellerAddress = $"{seller.Title} - {city.State.Title} - {city.Title} - {seller.Address}",
                    OrderItems = new List<OrderItemDetailForAdminQueryModel>()
                };

                // 5. آیتم‌ها
                foreach (var i in s.OrderItems)
                {
                    var productSell = await _productSellRepository.GetProductSellWithProductAsync(i.ProductSellId, cancellationToken);

                    var itemModel = new OrderItemDetailForAdminQueryModel
                    {
                        Id = i.Id,
                        OrderSellerId = i.OrderSellerId,
                        Price = i.Price,
                        PriceAfterOff = i.PriceAfterOff,
                        SumPrice = i.SumPrice,
                        SumPriceAfterOff = i.SumPriceAfterOff,
                        Count = i.Count,
                        Unit = i.Unit,
                        ProductId = productSell?.ProductId ?? 0,
                        ProductTitle = productSell?.Product?.Title ?? "",
                        ProductSellId = i.ProductSellId,
                        ProductImageName = productSell != null
                            ? $"{FileDirectories.ProductImageDirectory100}{productSell.Product.ImageName}"
                            : ""
                    };

                    sellerModel.OrderItems.Add(itemModel);
                }

                model.OrderSellers.Add(sellerModel);
            }

            // 6. کاربر سفارش
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null)
            {
                model.User = new()
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Mobile = user.Mobile,
                    Email = user.Email
                };
            }

            return model;
        }
    }
}
 
