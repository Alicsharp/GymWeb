using Gtm.Application.ShopApp.ProductGalleryApp.Command;
using Gtm.Application.ShopApp.ProductGalleryApp.Query;
using Gtm.Contract.ProductGalleryContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Product
{
    [Area("Admin")]
    public class ProductGalleryController : Controller
    {
        private readonly IMediator _mediator;

        public ProductGalleryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id)
        {
            var model = await _mediator.Send(new GetProductGalleriesForAdminQuery(id));
            return View(model.Value);
        }
        public async Task<IActionResult> Create(int id)
        {
            var model = new CreateProductGallery()
            {
                ProductId = id
            };
            return PartialView("Create", model);
        }
        [HttpPost]
        public async Task<JsonResult> Create(int id, CreateProductGallery model)
        {

            var res = await _mediator.Send(new CreateProductGalleryCommand(model));
            return new JsonResult(res.Value);
        }
        public async Task<bool> Delete(int id)
        {
            var res = await _mediator.Send(new ProductGalleryDeleteCommand(id));
            if (res.IsError) return false;
            return true;
        }
    }
}
