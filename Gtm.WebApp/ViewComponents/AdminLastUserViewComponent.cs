using Gtm.Application.AdminDashbord.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class AdminLastUserViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public AdminLastUserViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetLastUsersForAdminQuery());
            return View(model.Value);
        }
    }

}
