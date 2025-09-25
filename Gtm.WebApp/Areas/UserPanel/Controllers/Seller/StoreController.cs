using Gtm.Application.ShopApp.ProductSellApp.Command;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Application.StoresServiceApp.StroreApp.Command;
using Gtm.Application.StoresServiceApp.StroreApp.Query;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Contract.StoresContract.StoreContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers.Seller
{
    [Area("UserPanel")]
    [Authorize]
    public class StoreController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;

        public StoreController(IAuthService authService, IMediator mediator)
        {

            _authService = authService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int pageId = 1, int sellerId = 0, int take = 10, string filter = "")
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetStoresForUserPanelQuery(_userId=1, sellerId, filter, pageId, take));
            if (model.IsError) return NotFound();
            return View(model.Value);
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<JsonResult> GetSellers()
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new GetSellersForCreateStoreQuery(_userId));
            return Json(JsonConvert.SerializeObject(res.Value));
        }
        public async Task<JsonResult> GetSellerProducts(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new GetSellerProductsForCreateStoreQuery(id, _userId));
            return Json(JsonConvert.SerializeObject(res.Value));
        }
        [HttpPost]
        public async Task<bool> Create(string model)
        {
            var _userId = _authService.GetLoginUserId();
            var res = JsonConvert.DeserializeObject<CreateStore>(model);

            // بررسی قبل از ایجاد فروشگاه
            var isCheck = await _mediator.Send(new CheckCreateStoreCommand(res, _userId));
            if (isCheck.IsError)
                return false;

            // ایجاد فروشگاه و محصولات
            var result = await _mediator.Send(new CreateStoreCommand(_userId, res));
            if (result.IsError)
                return false;

            // به‌روزرسانی موجودی محصولات اگر وجود داشت
            if (res.Products != null && res.Products.Any())
            {
                await _mediator.Send(new EditProdoctSellAmountCommand(
                    res.Products.Select(p => new EditProdoctSellAmount
                    {
                        count = p.Count,
                        SellId = p.ProductSellId,
                        Type = p.Type
                    }).ToList()
                ));
            }

            return true;
        }



        public async Task<IActionResult> Detail(int id)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetStoreDetailForSellerPanelQuery(_userId, id));
            if (model.IsError) return NotFound();
            return View(model.Value);
        }
    }
}

