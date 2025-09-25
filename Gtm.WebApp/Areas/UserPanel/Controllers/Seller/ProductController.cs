using Gtm.Application.ShopApp.ProductApp.Query;
using Gtm.Application.ShopApp.ProductCategoryApp.Query;
using Gtm.Application.ShopApp.ProductSellApp.Command;
using Gtm.Application.ShopApp.ProductSellApp.Query;
using Gtm.Application.ShopApp.SellerApp.Command;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Contract.ProductSellContract.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers.Seller
{
    [Area("UserPanel")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly  IMediator _mediator;
        private readonly IAuthService _authService;
        private int _userId;

        public ProductController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
            
        }

        public async Task<IActionResult> Index(int id, int pageId = 1, string filter = "")
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetSellerProductsForUserPanelQuery(pageId, filter, id=1, _userId)) ;
            if (model.Value == null) return NotFound();
            return View(model.Value);
        }
        public async Task<IActionResult> Create(int id)
        {
            _userId = _authService.GetLoginUserId();
            var ok = await _mediator.Send(new IsSellerForUserCommand(id, _userId));
            if (ok.IsError==true) return NotFound();
            if (id == 0) return Redirect("/UserPanel/Seller/Index");
            CreateProductSell model = new()
            {
                SellerId = id
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateProductSell model)
        {
            _userId = _authService.GetLoginUserId();
            if (ModelState.IsValid == false) return View(model);
            if (model.SellerId != id) return NotFound();
            var ok = await _mediator.Send(new IsSellerForUserCommand(id, _userId));
            if (ok.IsError == true) return NotFound();
            var res = await _mediator.Send(new CreateProductSellCommand(model));
            if (res.IsError==false)
            {
                TempData["ok"] = true;
                return Redirect($"/UserPanel/Product/Index/{id}");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            _userId = _authService.GetLoginUserId();
            var ok = await _mediator.Send(new IsProductSellForUserCommand(_userId,id));
            if (ok.IsError) return NotFound();
            var model = await _mediator.Send(new GetForEditProductSellQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProductSell model)
        {
            _userId = _authService.GetLoginUserId();
            var ok = await _mediator.Send(new IsProductSellForUserCommand(_userId, id));
            if (id != model.Id || ok.IsError) return NotFound();
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditProductSellCommand(model));
            if (res.IsError==false)
            {
                TempData["ok"] = true;
                return Redirect($"/UserPanel/Product/Index/{model.SellerId}");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        [HttpPost]
        public async Task<JsonResult> Categories(int id = 0)
        {
            var res = await  _mediator.Send(new GetCategoryForAddProductSellsQuery(id));
            return Json(JsonConvert.SerializeObject(res));
        }
        [HttpPost]
        public async Task<JsonResult> GetProducts(int id)
        {
            if (id == 0)
            {
                List<ProductForAddProductSellQueryModel> model = new();
                return Json(JsonConvert.SerializeObject(model));
            }
            else
            {
                var res = await _mediator.Send(new GetProductsForAddProductSellsQuery(id));
                return Json(JsonConvert.SerializeObject(res));
            }

        }
    }
}
