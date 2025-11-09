using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gtm.WebApp.ViewComponents
{
    public class LogoResponsiveMenuHeaderViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public LogoResponsiveMenuHeaderViewComponent(IMediator mediator)
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

