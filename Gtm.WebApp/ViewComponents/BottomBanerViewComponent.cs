
using Gtm.Application.SiteServiceApp.BannerApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.ViewComponents
{
    public class BottomBanerViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public BottomBanerViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForUiQuery(1, BanerState.بنر_تکی_پایین_1680x210));
            return View(model.Value);
        }
    }
}
