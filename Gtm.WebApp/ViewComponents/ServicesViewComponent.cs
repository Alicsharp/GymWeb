using Gtm.Application.SiteServiceApp.SliderApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class ServicesViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public ServicesViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send( new Gtm.Application.SiteServiceApp.SiteServiceApp.Query.GetAllForUIQuery());
            return View(model.Value);
        }
    }
}
