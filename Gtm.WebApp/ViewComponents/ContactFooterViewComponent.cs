using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class ContactFooterViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public ContactFooterViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetContactDataForFooterQuery());
            return View(model.Value);
        }
    }
}
