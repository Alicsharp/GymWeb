using Gtm.Application.ShopApp.ProductApp.Command;
using Gtm.Application.ShopApp.ProductApp.Query;
using Gtm.Contract.ProductContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;


namespace Gtm.WebApp.Areas.Admin.Controllers.Product
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;

        public ProductController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int pageId = 1, int take = 10, int categoryId = 0, string filter = "", ProductAdminOrderBy orderBy = ProductAdminOrderBy.تاریخ_ثبت_از_آخر)
        {
            var res = await _mediator.Send(new GetProductsForAdminQuery(pageId, take, categoryId, filter, orderBy));
            return View(res.Value);
        }

        public async Task<IActionResult> Create()
        {
            var res = await _mediator.Send(new GetCategoriesForAddProductQuery());
            ViewData["Categories"] = res.Value;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProduct model)
        {
            var parents = await _mediator.Send(new GetCategoriesForAddProductQuery());
            ViewData["Categories"] = parents.Value;
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateProductCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var parents = await _mediator.Send(new GetCategoriesForAddProductQuery());
            ViewData["Categories"] = parents.Value;
            var model = await _mediator.Send(new GetProductForEditQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProduct model)
        {
            var parents = await _mediator.Send(new GetCategoriesForAddProductQuery());
            ViewData["Categories"] = parents.Value;
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new ProductEditCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var model = await _mediator.Send(new ProdouctActivationChangeCommand(id));
            if(model.IsError == false) return true;
            return false;
        }
    }
}
