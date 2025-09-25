
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class SocialViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public SocialViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetSocialForUiQuery());
            return View(model.Value);
        }
    }
}
