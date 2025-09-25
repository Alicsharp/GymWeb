
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class FavViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public FavViewComponent(IMediator mediator)
        {
            _mediator = mediator;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetFavIconForUiQuery());
            return View(model.Value);
        }
    }
}
