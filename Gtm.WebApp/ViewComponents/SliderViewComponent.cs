using Gtm.Application.SiteServiceApp.SliderApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class SliderViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public SliderViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetAllForUIQuery());
            return View(model.Value);
        }
    }
}
