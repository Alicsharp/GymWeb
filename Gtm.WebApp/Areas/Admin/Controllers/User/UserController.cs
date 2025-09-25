using Gtm.Application.UserApp.Command;
using Gtm.Application.UserApp.Query;
using Gtm.Contract.UserContract.Command;
using Gtm.Contract.UserContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Gtm.WebApp.Areas.Admin.Controllers.User
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int pageId = 1, int take = 20, string filter = "",UserOrderBySearch orderBy = UserOrderBySearch.تاریخ_ثبت_از_آخر_به_اول,UserStatusSearch status = UserStatusSearch.همه)
        {
            var model = await _mediator.Send(new GetUsersForAdminQuery(pageId, filter, take, orderBy, status));
            return View(model.Value);
        }
        [HttpGet]
        public IActionResult Create() => PartialView("Create", new CreateUserDto());
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            // اعتبارسنجی اولیه مدل
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new
                {
                    success = false,
                    errors = modelErrors
                });
            }

            // ارسال مدل به هندلر مدیاتور
            var result = await _mediator.Send(new CreateUserCommand(model));

            // بررسی خطاهای برگشتی از هندلر
            if (result.IsError)
            {
                var handlerErrors = result.Errors.ToDictionary(
                    error => string.IsNullOrWhiteSpace(error.Code) ? "General" : error.Code, // اگر نام فیلد نداشت
                    error => new[] { error.Description }
                );

                return Json(new
                {
                    success = false,
                    errors = handlerErrors
                });
            }

            // موفقیت
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _mediator.Send(new GetUserForEditByAdminQuery(id));
             return PartialView("Edit", model.Value);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserByAdmin model)
        {
            if (!ModelState.IsValid)
                return Json(new { Success = false, Message = "اطلاعات نامعتبر است." });

            var result = await _mediator.Send(new EditUserByAdminCommand(model));

            if (result.IsError)
                return Json(new { Success = false, Message = string.Join(", ", result.Errors) });

            return Json(new { Success = true, Message = "ذخیره شد." });
        }
        public async Task<bool> Active(int id)
        {
            var reulst = await _mediator.Send(new ActivationChangeCommand(id));
            if (reulst.IsError == false)
            {
                return true;
            }
            return false;
        }

    }
}
