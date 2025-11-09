using Gtm.Application.EmailServiceApp.EmailUserApp.Command;
using Gtm.Application.EmailServiceApp.MessageUserApp.Command;
using Gtm.Application.ShopApp.ProductApp.Query;
using Gtm.Application.ShopApp.WishListApp.Command;
using Gtm.Application.SiteServiceApp.SitePageApp.Query;
using Gtm.Application.SiteServiceApp.SiteServiceApp.Query;
using Gtm.Application.SiteServiceApp.SiteSettingApp.Query;
using Gtm.Contract.EmailContract.EmailUserContract.Command;
using Gtm.Contract.EmailContract.MessageUserContract.Command;
using Gtm.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IAuthService _authService;
        public HomeController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            _authService = authService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Error()
        {
            return View();
        }

        //بررس? شود
        [HttpPost]
        public async Task<string> AddEmailUser(string email)
        {
            var userId = _authService.GetLoginUserId();
            CreateEmailUser model = new()
            {
                Email = email,
                UserId = userId
            };
            var res = await _mediator.Send(new CreateEmailUserCommand(model));
            if (res.IsError == false) return "";
            return "";
        }
        [Route("/Contact")]
        public async Task<IActionResult> Contact()
        {
            var model = await _mediator.Send(new GetContactUsModelForUiQuery());
            return View(model.Value);
        }
        [Route("/Page/{slug}")]
        public async Task<IActionResult> Page(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();
            var model = await _mediator.Send(new GetSitePageQueryModelQuery(slug));
            if (model.Value == null) return NotFound();
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string fullName, string email, string subject, string message)
        {
            var userId = _authService.GetLoginUserId();
            if (string.IsNullOrEmpty(fullName) ||
                (string.IsNullOrEmpty(email) || (email.IsEmail() == false && email.IsMobile() == false))
                || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                TempData["FaildMessage"] = true;
                return RedirectToAction("Contact");
            }
            var res = await _mediator.Send(new CreateMessageCommand(new CreateMessageUser
            {
                Email = email.IsEmail() ? email : "",
                FullName = fullName,
                Message = message,
                PhoneNumber = email.IsMobile() ? email : "",
                Subject = subject,
                UserId = userId
            }));
            if (res.IsError == false)
            {
                TempData["SuccessMessage"] = true;
                return RedirectToAction("Contact");
            }
            else
            {
                TempData["FaildMessage"] = true;
                return RedirectToAction("Contact");
            }
        }
        [Route("/About")]
        public async Task<IActionResult> About()
        {
            var model = await _mediator.Send(new GetAboutUsModelForUiQuery());
            return View(model.Value);
        }
        public async Task<int> GetWishListCount()
        {
            var userId = _authService.GetLoginUserId();
            if (userId == 0)
            {
                string cookieName = "boloorShop-wishList-items";
                if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
                {
                    List<int> wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
                    if (wishesIds.Count > 0)
                    {
                        return wishesIds.Count;
                    }
                    else return 0;
                }
                else return 0;
            }
            else
            {
                string cookieName = "boloorShop-wishList-items";
                if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
                {
                    List<int> wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
                    if (wishesIds.Count > 0)
                    {
                       await _mediator.Send(new AddUsersWishListCommand(userId, wishesIds));
                        Response.Cookies.Delete(cookieName);
                    }
                }
                return await _mediator.Send(new GetUserWishListCountQuery(userId));
            }
        }
        [HttpGet]
        public async Task<bool> CheckProductWishList(int id)
        {
            var userId = _authService.GetLoginUserId();
            if (userId == 0)
            {
                // --- منطق کوکی شما (این بخش درست بود) ---
                string cookieName = "boloorShop-wishList-items";
                if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
                {
                    List<int> wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
                    if (wishesIds.Count > 0)
                    {
                        return wishesIds.Any(w => w == id);
                    }
                    else return false;
                }
                else return false;
            }
            else
            {
                // 1. آکولادها برای رفع خطای "Embedded statement" اضافه شدند
                var res = await _mediator.Send(new IsUserHaveProductWishListCommand(userId, id));

                // 2. بررسی نتیجه ErrorOr برای رفع خطای تبدیل
                if (res.IsError)
                {
                    // اگر خطای سیستمی رخ داد، 'false' (عدم وجود) را برمی‌گردانیم
                    return false;
                }

                // اگر خطایی نبود، مقدار bool واقعی (true یا false) را برمی‌گردانیم
                return res.Value;
            }
        }
        //[HttpGet]
        //public async Task<bool> UbsertProductWishList(int id)
        //{
        //    var userId = _authService.GetLoginUserId();
        //    if (userId == 0)
        //    {
        //        List<int> wishesIds = new List<int>();
        //        string cookieName = "boloorShop-wishList-items";
        //        if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
        //        {
        //            wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
        //            if (wishesIds.Count > 0 && wishesIds.Any(w => w == id))
        //            {
        //                var x = wishesIds.Single(w => w == id);
        //                wishesIds.Remove(x);
        //            }
        //            else
        //                wishesIds.Add(id);
        //        }
        //        else
        //            wishesIds.Add(id);
        //        Response.Cookies.Delete(cookieName);
        //        try
        //        {
        //            if (wishesIds.Count > 0)
        //            {
        //                var serializedList = System.Text.Json.JsonSerializer.Serialize(wishesIds);
        //                var cookieOptions = new CookieOptions
        //                {
        //                    Expires = DateTime.Now.AddDays(30),
        //                    HttpOnly = true,
        //                    Secure = true,
        //                    SameSite = SameSiteMode.Strict
        //                };

        //                Response.Cookies.Append(cookieName, serializedList, cookieOptions);
        //            }
        //            return true;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {

        //        var res = await _mediator.Send(new AddUserProductWishListCommand(userId, id));
        //        // 2. بررسی نتیجه ErrorOr برای رفع خطای تبدیل
        //        if (res.IsError)
        //        {
        //            // اگر خطای سیستمی رخ داد، 'false' (عدم وجود) را برمی‌گردانیم
        //            return false;
        //        }
        //    }

        //}
        //[HttpGet]
        //public IActionResult WishList()
        //{
        //    var userId = _authService.GetLoginUserId();
        //    List<WishListProductQueryModel> model = new();
        //    if (userId == 0)
        //    {
        //        string cookieName = "boloorShop-wishList-items";
        //        if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
        //        {
        //            List<int> wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
        //            if (wishesIds.Count > 0)
        //            {
        //                model = _productUiQuery.GetWishListForUserFromCppkie(wishesIds);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        string cookieName = "boloorShop-wishList-items";
        //        if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
        //        {
        //            List<int> wishesIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(cartJson);
        //            if (wishesIds.Count > 0)
        //            {
        //                _wishListApplication.AddUsersWishList(userId, wishesIds);
        //                Response.Cookies.Delete(cookieName);
        //            }
        //        }
        //        model = _productUiQuery.GetWishListForUserLoggedIn(userId);
        //    }
        //    return View(model);
        //}
        //[HttpPost]
        //public async Task<JsonResult> AjaxSearch(string filter)
        //{
        //    List<SearchAjaxQueryModel> model = new();
        //    if (!string.IsNullOrEmpty(filter))
        //    {
        //       var res= await _mediator.Send(new AjaxSearchQuery(filter));
        //        var product = 
        //            .SearchAjax(filter);
        //        if (product.Count() > 0)
        //            model.AddRange(product.Select(p => new SearchAjaxQueryModel
        //            {
        //                ImageAddress = p.ImageAddress,
        //                Url = $"/Product/{p.id}/{p.Slug}",
        //                Title = p.Title,
        //            }).ToList());
        //        if (model.Count() < 10)
        //        {
        //            int count = 10 - model.Count;
        //            var blogs = _blogUiQuery.SearchAjax(filter, count);
        //            if (blogs.Count() > 0)
        //                model.AddRange(blogs.Select(p => new SearchAjaxQueryModel
        //                {
        //                    ImageAddress = p.ImageAddress,
        //                    Url = $"/Blog/{p.Slug}",
        //                    Title = p.Title,
        //                }).ToList());
        //        }
        //    }
        //    return Json(JsonConvert.SerializeObject(model));
        //}
    }
}