using Gtm.Application.AdminDashbord.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class AdminLastOrderViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public AdminLastOrderViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetLastOrdersForAdminQuery());
            return View(model.Value);
        }
    }


}
