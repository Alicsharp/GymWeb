 
using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Application.ShopApp.SellerApp.Command;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Contract.SellerContract.Command;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Seller
{
    [Area("Admin")]
    public class SellerController : Controller
    {
        private readonly IMediator _mediator;

        public SellerController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index(int pageId = 1, int take = 10, string filter = "")
        {
            var model = await _mediator.Send(new GetSellersForAdminQuery(pageId, take, filter));
            return View(model.Value);
        }
        public async Task<IActionResult> Requests()
        {
            var model = await _mediator.Send(new GetSellerRequestsForAdminQuery());
            return View(model.Value);
        }
        public async Task<IActionResult> Request(int id)
        {
            var model = await _mediator.Send(new GetSellerRequestDetailForAdminQuery(id));
            return View(model.Value);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var model = await _mediator.Send(new GetSellerDetailForAdminQuery(id));
            if (model.Value.Status == SellerStatus.درخواست_تایید_نشده || model.Value.Status == SellerStatus.درخواست_ارسال_شده)
            {
                return Redirect($"/Admin/Seller/Request/{id}");
            }
            return View(model.Value);
        }
        public IActionResult Accept(int id) => PartialView("Accept", new ChangeSellerStatusByAdmin()
        {
            Id = id
        });
        [HttpPost]
        public async Task<JsonResult> Accept(ChangeSellerStatusByAdmin model)
        {
            if (string.IsNullOrEmpty(model.DescriptionSMS))
            {
                string ress = "لطفا متن اس ام اس را وارد کنید .";
                return Json(ress);
            }
            else
            {
                var res = await _mediator.Send(new ChangeSellerStatusCommand(model.Id, SellerStatus.درخواست_تایید_شده));
                if (res.Value == true)
                {
                    string resss = "علمیات درست بود";
                    return Json(resss);
                }
                else
                {
                    string rsesss = "علمیات نادرست بود تماس بگیرید بود";
                    return Json(rsesss);

                }

            }

        }
        public IActionResult Reject(int id) => PartialView("Reject", new ChangeSellerStatusByAdmin()
        {
            Id = id
        });
        [HttpPost]
        public async Task<JsonResult> Reject(ChangeSellerStatusByAdmin model)
        {
            if (string.IsNullOrEmpty(model.DescriptionSMS))
            {
                string ress = "لطفا متن اس ام اس را وارد کنید .";
                return Json(ress);
            }
            else
            {
                var res = await _mediator.Send(new ChangeSellerStatusCommand(model.Id, SellerStatus.درخواست_تایید_شده));
                if (res.Value == true)
                {
                    string resss = "علمیات درست بود";
                    return Json(resss);
                }
                else
                {
                    string rsesss = "علمیات نادرست بود تماس بگیرید بود";
                    return Json(rsesss);

                }
            }

        }
    }

}
