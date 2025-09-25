
using Gtm.Application.SiteServiceApp.BannerApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.ViewComponents
{
    public class CenterBaner2ViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public CenterBaner2ViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForUiQuery(2, BanerState.بنر_2تایی_وسط_820x328));
            return View(model.Value);
        }
    }
}
