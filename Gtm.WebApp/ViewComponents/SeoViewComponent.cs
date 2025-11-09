using Gtm.Application.SeoApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.WebApp.ViewComponents
{
    public class SeoViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;

        public SeoViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(int ownerId, WhereSeo where, string title)
        {
            var model = await _mediator.Send(new GetSeoQuery(ownerId, where, title));
            return View(model.Value);
        }
    }
}
