using Gtm.Application.SiteServiceApp.ImageSiteApp.Command;
using Gtm.Application.SiteServiceApp.ImageSiteApp.Query;
using Gtm.Application.SiteServiceApp.SiteSettingApp.Command;
using Gtm.Contract.SiteContract.ImageSiteContract.Command;
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Areas.Admin.Controllers.Site
{
    [Area("Admin")]
    public class SiteController : Controller
    {
        private readonly IMediator _mediator;

        public SiteController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetForUbsertCommand());
            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UbsertSiteSetting model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateUbsertCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
        public async Task<IActionResult> Images(int pageId = 1, int take = 10, string filter = "")
        {
            var res = await _mediator.Send(new GetImageAllForAdminQuery(pageId, take, filter));
            return View(res.Value);

        }

        //[HttpPost]
        //public async Task<IActionResult> CreateImage(IFormFile image, string title)
        //{
        //    if (image == null || image.IsImage() == false)
        //    {
        //        ViewBag.message = ValidationMessages.ImageErrorMessage; ;
        //        return RedirectToAction("Images");
        //    }
        //    if (string.IsNullOrEmpty(title))
        //    {
        //        ViewBag.message = "عنوان اجباری است";
        //        return RedirectToAction("Images");
        //    }
        //    var res = await _mediator.Send(new CreateImageSiteCommand(new CreateImageSite() { Title = title, ImageFile = image }));

        //    if (res.IsError == false)
        //    {
        //        TempData["ok"] = true;
        //        return RedirectToAction("Images");
        //    }
        //    ViewBag.message = res.IsError.ToString();
        //    return RedirectToAction("Images");
        //}

        public IActionResult CreateImage() => View();

        [HttpPost]
        public async Task<IActionResult> CreateImage(CreateImageSite model)
        {
            if (!ModelState.IsValid) return View(model);
            var res = await _mediator.Send(new CreateImageSiteCommand(model));
            if (res.IsError == false)
            {
                TempData["ok"] = true;
                return RedirectToAction("Images");
            }
            ModelState.AddModelError("", "");
            return View(model);
        }
    }
}
