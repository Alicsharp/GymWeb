
using Gtm.Application.ArticleApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class LastArticleViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public LastArticleViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetLastArticleForMagUiQuery());
            return View(model.Value);
        }
    }
}

