using Gtm.Application.AdminDashbord.Query;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gtm.WebApp.ViewComponents
{
    public class AdminDataViewComponent:ViewComponent
    {
        private readonly IMediator  _mediator;

        public AdminDataViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetAdminDataQuery());
 
            return View(model.Value);
        }
    }


}
