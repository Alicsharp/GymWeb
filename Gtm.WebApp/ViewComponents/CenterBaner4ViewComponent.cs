
using Gtm.Application.SiteServiceApp.BannerApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.ViewComponents
{
    public class CenterBaner4ViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public CenterBaner4ViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetForUiQuery(4, BanerState.بنر_4تایی_وسط_400x300));
            return View(model.Value);
        }
    }
}
