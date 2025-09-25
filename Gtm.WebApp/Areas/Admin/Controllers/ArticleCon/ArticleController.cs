using Gtm.Application.ArticleApp.Command;
using Gtm.Application.ArticleApp.Query;
using Gtm.Application.ArticleCategoryApp.Query;
using Gtm.Contract.ArticleContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utility.Appliation;

namespace Gtm.WebApp.Areas.Admin.Controllers.ArticleCon
{
    [Area("Admin")]
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly IMediator _mediator;

        public ArticleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int id = 0)
        {
            var result = await _mediator.Send(new GetArticleForAdminQuery(id));
            return View(result.Value);
        }
        public async Task<IActionResult> Create(int id = 0)
        {
            var categoriesResult = await _mediator.Send(new GetArticleCategoriesForAddArticleQuery());

            if (categoriesResult.IsError)
            {
                // مدیریت خطا
                return View("Error");
            }

            ViewData["Categories"] = categoriesResult.Value;

            return View(new CreateArticleDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateArticleDto model)
        {
 
            //if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateArticleCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return Redirect($"/Admin/Article/Index/0");
            }
            ModelState.AddModelError("", ValidationMessages.ChildCategoryMessage);
            TempData["faild"] = true;
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //var parents = await _mediator.Send(new GetArticleCategoriesForAddArticleQuery());
            //ViewBag.Categories = parents.Value;

            //var model = await _mediator.Send(new GetForEditArticleQuery(id));

            //// بررسی کنید که آیا خطایی وجود دارد یا خیر
            //if (model.IsError)
            //{
            //    // اگر مقاله یافت نشد، به لیست مقالات ریدایرکت کنید یا پیام خطا نمایش دهید
            //    TempData["ErrorMessage"] = "مقاله مورد نظر یافت نشد.";
            //    return RedirectToAction("Index"); // یا هر اکشن دیگری
            //}
            //return View(model.Value);
            var article = await _mediator.Send(new GetForEditArticleQuery(id));
            var categories = await _mediator.Send(new GetArticleCategoriesForAddArticleQuery());

            ViewData["Parents"] = categories.Value; // همین اسم رو تو ویو استفاده کن
            return View(article.Value);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(  UpdateArticleDto model)
        {
     
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new EditArticleCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("dsfdsf", "fddsfds");
            return View(model);
        }
        public async Task<bool> Active(int id)
        {
            var result = await _mediator.Send(new ChangeArticleActivationCommand (id));
            if (result.IsError == false)
            {
                return true;
            }
            return false;
        }
        public async Task<JsonResult> GetCategories(int id)
        {
            var model = await _mediator.Send(new GetArticleCategoriesForAddArticleQuery());
            var res = JsonConvert.SerializeObject(model.Value);
            return Json(res);
        }
    }
}
