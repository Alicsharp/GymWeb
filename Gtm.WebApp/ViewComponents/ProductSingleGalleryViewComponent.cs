using Gtm.Application.ShopApp.ProductGalleryApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Gtm.WebApp.ViewComponents
{
    public class ProductSingleGalleryViewComponent: ViewComponent
    {
        private readonly IMediator _mediator;

        public ProductSingleGalleryViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            var model = await _mediator.Send(new GetProductSingleGalleryQuery(productId));
            return View(model.Value);
        }
    }
}
