
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using Gtm.WebApp.Utility;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Discount
{
    //[Area("Admin")]
    //[PermissionChecker(UserPermission.مدیریت_تخفیفات)]
    //public class OrderDiscountController : Controller
    //{
    //    private readonly IMediator _mediator;

    //    public OrderDiscountController(IMediator mediator)
    //    {
    //        _mediator = mediator;
    //    }

    //    public async Task<IActionResult> Active() => View("Active",await _mediator.Send(new GetAllActivesForAdminQuery()));
    //    public async Task<IActionResult> InActive() => View("Active", await _mediator.Send(new GetAllInActivesForAdminQuery()));
    //    public IActionResult Create() => PartialView("Create", new CreateOrderDiscount()
    //    {
    //        EndDate = DateTime.Now.AddDays(1).ToPersainDatePicker(),
    //        StartDate = DateTime.Now.ToPersainDatePicker()
    //    });
    //    [HttpPost]
    //    public async Task<IActionResult> Create(CreateOrderDiscount model)
    //    {
    //        OperationResult res = new(false);
    //        if (!ModelState.IsValid)
    //            res = new(false, "لطفا اطلاعات را درست وارد کنید .");
    //        else
    //            res = await  _mediator.Send(new CreateOrderDiscountCommand(model, OrderDiscountType.Order, 0));
    //        return Json(JsonConvert.SerializeObject(res));
    //    }
    //    public async Task<IActionResult> Edit(int id)
    //    {
    //        var model = await _mediator.Send(new GetForEditOrderDiscountQuery(id));
    //        if (model.Value == null) return NotFound();
    //        return PartialView("Edit", model);
    //    }
    //    [HttpPost]
    //    public async Task<IActionResult> Edit(int id, EditOrderDiscount model)
    //    {
    //        var ress = await _mediator.Send(new GetForEditOrderDiscountQuery(id));
    //        if (ress.Value == null) return NotFound();
    //        OperationResult res = new(false);
    //        if (!ModelState.IsValid || model.Id != id)
    //            res = new(false, "لطفا اطلاعات را درست وارد کنید .");
    //        else
    //            res = await _mediator.Send(new EditOrderDiscountCommand(model));
    //        return Json(JsonConvert.SerializeObject(res));
    //    }
    //}
    [Area("Admin")]
    //[PermissionChecker(UserPermission.مدیریت_تخفیفات)] // اگه PermissionChecker نیاز به IMediator نداره، درست کار می‌کنه
    public class OrderDiscountController : Controller
    {
        private readonly IMediator _mediator;

        public OrderDiscountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Active()
        {
            var model = await _mediator.Send(new GetAllActivesForAdminQuery());
            return View("Active", model);
        }

        public async Task<IActionResult> InActive()
        {
            var model = await _mediator.Send(new GetAllInActivesForAdminQuery());
            return View("Active", model);
        }

        public IActionResult Create()
        {
            var model = new CreateOrderDiscount()
            {
                EndDate = DateTime.Now.AddDays(1).ToPersainDatePicker(),
                StartDate = DateTime.Now.ToPersainDatePicker()
            };
            return PartialView("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDiscount model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "لطفا اطلاعات را درست وارد کنید." });

            var result = await _mediator.Send(new CreateOrderDiscountCommand(model, OrderDiscountType.Order, 0));

            if (result.IsError)
                return Json(new { success = false, message = result.FirstError.Description });

            return Json(new { success = true });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _mediator.Send(new GetForEditOrderDiscountQuery(id));

            if (model.IsError || model.Value == null)
                return NotFound();

            return PartialView("Edit", model.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditOrderDiscount model)
        {
            if (!ModelState.IsValid || model.Id != id)
                return Json(new { success = false, message = "لطفا اطلاعات را درست وارد کنید." });

            var result = await _mediator.Send(new EditOrderDiscountCommand(model));

            if (result.IsError)
                return Json(new { success = false, message = result.FirstError.Description });

            return Json(new { success = true });
        }
    }
}
 
