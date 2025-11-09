
using Gtm.Application.ArticleApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class BestBlogMagViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        public BestBlogMagViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetBestArticleForUiQuery());
            return View(model.Value);
        }
    }

}

