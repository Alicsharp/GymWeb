using Gtm.Application.AdminDashbord.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Gtm.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[PermissionChecker(UserPermission.پنل_ادمین)]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> GetChartData(string year = "0")
        {
            var model = await _mediator.Send(new GetTransactionChartDataQuery(year));
            var json = JsonConvert.SerializeObject(model.Value);
            return Json(json);
        }
        //[HttpPost]
        //public JsonResult GetNotification(string year = "0")
        //{
        //    var model = _adminQuery.GetNotificationForAdmin();
        //    var json = JsonConvert.SerializeObject(model);
        //    return Json(json);
        //}

        //[HttpPost]
        //public JsonResult GetMessageNotifications()
        //{
        //    var model = _messageUserAdminQuery.GetUnSeenUserMessagesFotNotifes();
        //    var jsom = JsonConvert.SerializeObject(model);
        //    return Json(jsom);
        //}
    }
}
