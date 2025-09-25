using Gtm.Application.SiteServiceApp.MenuApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class MenuFooterViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public MenuFooterViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForFooterQuery());

            return View(model.Value);
        }
    }
}
