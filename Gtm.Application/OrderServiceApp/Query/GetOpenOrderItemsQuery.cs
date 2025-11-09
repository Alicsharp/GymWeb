using Gtm.Contract.OrderContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.OrderServiceApp.Query
{
    public record GetOpenOrderItemsQuery(int UserId) : IRequest<List<ShopCartViewModel>>;
    public class GetOpenOrderItemsQueryHandler : IRequestHandler<GetOpenOrderItemsQuery, List<ShopCartViewModel>>
    {
        private readonly IOrderRepository _orderRepository;
        // (فرض می‌کنیم FileDirectories یک کلاس static یا const است)

        public GetOpenOrderItemsQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<ShopCartViewModel>> Handle(GetOpenOrderItemsQuery request, CancellationToken cancellationToken)
        {
            List<ShopCartViewModel> model = new();

            // 1. واکشی تمام داده‌ها با یک تماس به ریپازیتوری
            var order = await _orderRepository.GetOpenOrderWithDetailsAsync(request.UserId);

            if (order == null)
                return model; // بازگشت لیست خالی

            // 2. اجرای منطق بیزینس (تبدیل) بدون هیچ تماس اضافه با دیتابیس
            foreach (var seller in order.OrderSellers)
            {
                // اطلاعات فروشگاه از قبل بارگذاری شده است
                var shop = seller.Seller;

                foreach (var item in seller.OrderItems)
                {
                    // اطلاعات محصول از قبل بارگذاری شده است
                    var productSell = item.ProductSell;

                    // (بررسی نال بودن برای اطمینان بیشتر)
                    if (shop == null || productSell == null || productSell.Product == null)
                    {
                        // می‌توانید این مورد را لاگ کنید، چون نشان‌دهنده داده ناقص است
                        continue;
                    }

                    model.Add(new ShopCartViewModel
                    {
                        priceAfterOff = item.PriceAfterOff,
                        count = item.Count,
                        imageName = FileDirectories.ProductImageDirectory500 + productSell.Product.ImageName,
                        price = item.Price,
                        productId = productSell.ProductId,
                        ProductSellerIds = item.ProductSellId,
                        shopTitle = shop.Title,
                        slug = productSell.Product.Slug,
                        title = productSell.Product.Title,
                        unit = item.Unit
                    });
                }
            }

            return model;
        }
    }
}
