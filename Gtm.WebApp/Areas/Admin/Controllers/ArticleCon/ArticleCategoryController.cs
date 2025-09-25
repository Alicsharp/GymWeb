using Gtm.Application.ArticleCategoryApp.Command;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Contract.ArticleCategoryContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Appliation;

namespace Gtm.WebApp.Areas.Admin.Controllers.ArticleCon
{
    [Area("Admin")]
    public class ArticleCategoryController : Controller
    {
        private readonly IMediator _mediator;

        public ArticleCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id=0 )
        {
            if (id > 0)
            {
                var result = await _mediator.Send(new CheckCategoryHaveParentQuery(id));
               if( result.IsError) return NotFound();
            }


            var model = await _mediator.Send(new GetCategoriesForAdminQuery(id));
           
            return View(model.Value);

        }
        public IActionResult Create(int? Id=null )
        {
            var model = new CreateArticleCategory { Parent =null};
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateArticleCategory model)
        {
            //if (id != model.ParentId) return NotFound();
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateArticleCategoryCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/ArticleCategory/Index/{model.Parent}");
            }
            ModelState.AddModelError("", ValidationMessages.ChildCategoryMessage);
            TempData["faild"] = true;
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            
            var model = await _mediator.Send(new GetArticleCategoryForEditQuery(id));
            var s = model.Value;
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(  UpdateArticleCategoryDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditArticleCategoryCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/ArticleCategory/Index/{model.Parent}");
            }
            ModelState.AddModelError("", ValidationMessages.ChildCategoryMessage);
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var result = await _mediator.Send(new ArticleCategoryActiveChangeCommand(id));
            if (result.IsError == false)
            {
                return true;
            }
            return false;
        }
    }
}
