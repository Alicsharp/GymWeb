using Gtm.Application.SiteServiceApp.MenuApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class MenuIndexViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public MenuIndexViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForIndexQuery());
            return View(model.Value);
        }
    }
}
