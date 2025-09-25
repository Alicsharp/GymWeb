
using Gtm.Application.ShopApp.ProductCategoryApp.Command;
using Gtm.Application.ShopApp.ProductCategoryApp.Query;
using Gtm.Contract.ProductCategoryContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Product
{
    [Area("Admin")]
    public class ProductCategoryController : Controller
    {
        private readonly IMediator _mediator;
        public ProductCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id = 0)
        {

            var have = await _mediator.Send(new CheckProductCategoryHaveParentCommand(id));
            if (id > 0 && have.IsError==true) return NotFound();
            var result = await _mediator.Send(new GetProductCategoriesForAdminQuery(id));
            return View(result.Value);
        }
        public async Task<IActionResult> Create(int id = 0)
        {
            var have = await _mediator.Send(new CheckProductCategoryHaveParentCommand(id));
            if (id > 0 && have.IsError==true) return NotFound();
            return View(new CreateProductCategory()
            {
                Parent = id
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateProductCategory model)
        {
            if (id != model.Parent) return NotFound();
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new ProductCategoryCreateCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/ProductCategory/Index/{id}");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _mediator.Send(new GetProductCategoryForEditQuery(id));
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProductCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditProductCategoryCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/ProductCategory/index/0");
            }
            //ModelState.AddModelError(res.ModelName, res.Message);
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var res = await _mediator.Send(new ProductCategoryActivationChangeCommand(id));
            if (res.IsError == false)
            return true;
            return false;
        }
    }
}
