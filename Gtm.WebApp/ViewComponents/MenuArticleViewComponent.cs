
using Gtm.Application.SiteServiceApp.MenuApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class MenuArticleViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public MenuArticleViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForArticleQuery());
            return View(model.Value);
        }
    }
}
