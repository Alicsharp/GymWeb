using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query;
using Gtm.Application.OrderServiceApp.Command;
using Gtm.Application.OrderServiceApp.Query;
using Gtm.Application.ShopApp.SellerApp.Command;
using Gtm.Contract.OrderContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Area("UserPanel")]
    [Authorize]
    public class OrderController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;

        public OrderController(IAuthService authService, IMediator mediator)
        {

            _authService = authService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Order()
        {
            _userId = _authService.GetLoginUserId();
            List<ShopCartViewModel> model = new();
            string cookieName = "boloorShop-cart-items";
            if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
            {
                model = JsonSerializer.Deserialize<List<ShopCartViewModel>>(cartJson);
                var ok = await _mediator.Send(new UbsertOpenOrderForUserCommand(_userId, model));
                Response.Cookies.Delete(cookieName);
            }
            await   _mediator.Send(new CheckOrderItemDataCommand(_userId));
            await _mediator.Send(new CheckOrderEmptyCommand(_userId));
            var res = await _mediator.Send(new GetOpenOrderForUserQuery(_userId));
            if (res.Value == null)
            {
                TempData["noOpenOrder"] = true;
                return Redirect("/Shop");
            }
            return View(res.Value);
        }
        public async Task<bool> DeleteOrderItem(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new DeleteOrderItemCommand(id, _userId));
            if(res.IsError)
                return false;
            return true;
        }
        public async Task<IActionResult> OrderItemMinus(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new OrderItemMinusCommand(id, _userId));
            var model = JsonSerializer.Serialize(res.Value);
            return Json(model);
        }
        public async Task<IActionResult> OrderItemPlus(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res =   await _mediator.Send(new OrderItemPlusCommand(id, _userId));
            var model = JsonSerializer.Serialize(res.Value);
            return Json(model);
        }
        [HttpPost]
        public async Task<JsonResult> AddOrderSellerDiscount(int id, string code)
        {
            var userId = _authService.GetLoginUserId();

            // بررسی اینکه کاربر سفارش باز برای این فروشنده دارد یا خیر
            var orderResult = await _mediator.Send(new HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery(userId, id));
            if (orderResult.IsError)
            {
                return Json(new { Success = false, Message = orderResult.FirstError.Description });
            }

            // اینجا باید سرویس تخفیف رو صدا بزنی تا اطلاعات کد تخفیف گرفته بشه
            // فرض می‌کنیم متدی داری که بر اساس code جزئیات تخفیف رو برمی‌گردونه
            var discountResult = await _mediator.Send(new GetDiscountByCodeQuery(id, code));
            if (discountResult.IsError)
            {
                return Json(new { Success = false, Message = discountResult.FirstError.Description });
            }

            var discount = discountResult.Value;

            // اضافه کردن تخفیف به سفارش
            var addDiscountResult = await _mediator.Send(
                new AddOrderSellerDiscountCommand(userId, id, discount.Id, discount.Title, discount.Percent)
            );

            if (addDiscountResult.IsError)
            {
                return Json(new { Success = false, Message = addDiscountResult.FirstError.Description });
            }

            // کم کردن یک استفاده از تخفیف
            var minusResult = await _mediator.Send(new MinusUseOrderDiscountCommand(discount.Id));
            if (minusResult.IsError)
            {
                return Json(new { Success = false, Message = minusResult.FirstError.Description });
            }

            return Json(new { Success = true, Message = "تخفیف با موفقیت اعمال شد." });
        }
        //[HttpPost]
        //public async Task<JsonResult> AddOrderDiscount(string code)
        //{
        //    OperationResult model = new(false);
        //    _userId = _authService.GetLoginUserId();
        //    bool ok = await _orderUserPanelQuery.HaveUserOpenOrderAsync(_userId);
        //    if (ok)
        //    {
        //        OperationResultOrderDiscount res = await _orderDiscountApplication.GetOrderDiscountForAddOrderdiscountAsync(code);
        //        if (res.Success)
        //        {
        //            bool add = await _orderApplication.AddOrderDiscountAsync(_userId, res.Id, res.Title, res.Percent);
        //            if (add)
        //            {
        //                model.Success = true;
        //                model.Message = res.Message;
        //            }
        //            else
        //            {
        //                await _orderDiscountApplication.MinusUseAsync(res.Id);
        //                model.Message = "عملیات نا موفق !! مجددا تلاش کنید ";
        //            }
        //        }
        //        else
        //            model.Message = res.Message;
        //    }
        //    else
        //        model.Message = "شما فاکتور بازی ندارید .";
        //    var json = JsonSerializer.Serialize(model);
        //    return Json(json);
        //}
    }
}


