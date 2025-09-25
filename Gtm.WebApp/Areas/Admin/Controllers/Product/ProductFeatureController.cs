
using Gtm.Application.ShopApp.ProductFeatureApp.Command;
using Gtm.Application.ShopApp.ProductFeatureApp.Query;
using Gtm.Contract.ProductFeautreContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Product
{
    [Area("Admin")]
    public class ProductFeatureController : Controller
    {

        private readonly IMediator _mediator;

        public ProductFeatureController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id)
        {
            var model = await _mediator.Send(new GetProductFeaturesForAdminQuery(id));
            return View(model.Value);
        }
        public async Task<IActionResult> Create(int id)
        {
            var model = new CreateProductFeautre()
            {
                ProductId = id
            };
            return PartialView("Create", model);
        }
        [HttpPost]
        public async Task<JsonResult> Create(int id, CreateProductFeautre model)
        {
            if (id != model.ProductId)
            {
                return new JsonResult(new { success = false, message = "لطفا اطلاعات را درست وارد کنید." })
                {
                    StatusCode = 400 // BadRequest
                };
            }

            var res = await _mediator.Send(new ProductFeautreCreateCommand(model));
            return new JsonResult(res.Value);
        }
        public async Task<bool> Delete(int id)
        {
            var res = await _mediator.Send(new DeleteProductFeatureCommand(id));
            if(res.IsError)
            {
                return false;
            }
            return true;

        }
    }
}
