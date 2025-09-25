
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class LogoHeaderViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public LogoHeaderViewComponent(IMediator mediator)
        {
            _mediator = mediator;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetLogoForUiQuery());
            return View(model.Value);
        }
    }
}
