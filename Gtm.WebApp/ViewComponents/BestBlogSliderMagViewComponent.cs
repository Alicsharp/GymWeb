
using Gtm.Application.ArticleApp.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.ViewComponents
{
    public class BestBlogSliderMagViewComponent : ViewComponent
    {

        private readonly IMediator _mediator;
        public BestBlogSliderMagViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetBestArticleForSliderUiQuery());
            return View(model.Value);
        }
    }
}

