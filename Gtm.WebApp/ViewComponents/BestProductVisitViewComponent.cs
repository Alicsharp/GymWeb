using Gtm.Application.ShopApp.ProductVisitApp.Query;
using Gtm.Application.ShopApp.WishListApp.Command;
using Gtm.Application.ShopApp.WishListApp.Query;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.ViewComponents
{
    public class BestProductVisitViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        // (ما دیگر به IProductUiQuery و IWishListQuery نیازی نداریم)

        public BestProductVisitViewComponent(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        // 1. متد باید به InvokeAsync تغییر کند
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 2. فراخوانی کوئری اول با MediatR
            // (این همان هندلری است که در مراحل قبلی ساختیم)
            var modelResult = await _mediator.Send(new GetMostVisitedProductsForIndexQuery());

            // 3. مدیریت نتیجه ErrorOr
            if (modelResult.IsError)
            {
                // اگر خطایی رخ داد (مثلاً محصولی یافت نشد)، یک لیست خالی به View برگردان
                return View(new List<ProductCartForIndexQueryModel>());
            }

            var model = modelResult.Value;

            // 4. گرفتن شناسه کاربر (این منطق تغییری نمی‌کند)
            var userId = _authService.GetLoginUserId();

            // 5. اجرای حلقه برای بررسی WishList
            foreach (var item in model)
            {
                if (userId == 0)
                {
                    // منطق کوکی (این منطق تغییری نمی‌کند)
                    string cookieName = "boloorShop-wishList-items";
                    if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
                    {
                        List<int> wishesIds = JsonSerializer.Deserialize<List<int>>(cartJson);
                        if (wishesIds.Count > 0)
                        {
                            item.isWishList = wishesIds.Any(w => w == item.Id);
                        }
                        else item.isWishList = false;
                    }
                    else item.isWishList = false;
                }
                else
                {
                    // 6. فراخوانی کوئری دوم (داخل حلقه N+1) با MediatR
                    // (این همان هندلری است که توافق کردیم 'bool' برگرداند)
                   var res= await _mediator.Send(new IsUserHaveProductWishListCommand(userId, item.Id));
                    item.isWishList = res.Value;
                }
            }

            return View(model);
        }
    }

}

