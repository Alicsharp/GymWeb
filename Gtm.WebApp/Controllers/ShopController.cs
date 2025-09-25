using Gtm.Application.ShopApp.ProductApp.Query;
using Gtm.Contract.OrderContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Controllers
{
    public class ShopController : Controller
    {
        private readonly IMediator _mediator;
        public ShopController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("/Shop/{id?}")]
        public async Task<IActionResult> Index(int? id, string slug = "", int pageId = 1, string filter = "", ShopOrderBy orderBy = ShopOrderBy.جدید_ترین)
        {
            var model = await _mediator.Send(new GetProductsForUiQuery(pageId, filter, slug, id == null ? 0 : id.Value, orderBy));
            return View(model.Value);
        }
        [Route("/Cart")]
        public IActionResult Cart()
        {
            List<ShopCartViewModel> model = new();
            string cookieName = "boloorShop-cart-items";
            if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
            {
                // تبدیل JSON به لیست محصولات
                model = JsonSerializer.Deserialize<List<ShopCartViewModel>>(cartJson);

            }
            return View(model);
        }

        [Route("/Product/{id}/{slug}")]
        public async Task<IActionResult> Single(int id, string slug)
        {
            var model = await _mediator.Send(new GetSingleProductForUiQuery(id));
            if (model.Value == null || model.IsError == true) return NotFound();
            if (model.Value.Slug != slug) return Redirect($"/Product/{id}/{model.Value.Slug}");
            return View(model.Value);
        }
    }
}
