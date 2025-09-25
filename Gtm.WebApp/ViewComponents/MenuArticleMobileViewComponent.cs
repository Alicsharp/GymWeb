
using Gtm.Application.SiteServiceApp.MenuApp.Query;
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class MenuArticleMobileViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public MenuArticleMobileViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var social = await _mediator.Send(new GetSocialForUiQuery());
            var menus = await _mediator.Send(new GetForArticleQuery());
            return View(Tuple.Create(menus.Value, social.Value));
        }

    }
}
