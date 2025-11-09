using Gtm.Application.ArticleApp.Query;
 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IMediator _mediator;

        public ArticleController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("")]
        [HttpGet("category/{categorySlug}")]
        public async Task<IActionResult> Index(string categorySlug = "",int page = 1,string filter = "")
        {
            var model = await _mediator.Send(new GetArticleForUiQuery(
                slug: categorySlug,
                pageId: page,
                filter: filter));

            return View(model.Value);
        }
        [HttpGet("details/{articleSlug}")]
        public async Task<IActionResult> Details(string articleSlug)
        {
            var model = await _mediator.Send(new GetSingleArticleForUiQuery(articleSlug));
            if (model.Value == null)
                return NotFound();

            return View(model.Value);
        }

    }
}       