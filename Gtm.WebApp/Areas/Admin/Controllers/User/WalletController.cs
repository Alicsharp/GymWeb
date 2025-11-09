using Gtm.Application.TransactionServiceApp.Query;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.WalletContract.Command;
using Gtm.WebApp.Utility;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.User
{
    [Area("Admin")]
    //[PermissionChecker (UserPermission.کیف_پول)]
    public class WalletController : Controller
    {
        private readonly IMediator _mediator;
        private int _userId;
        private readonly IAuthService _authService;

        public WalletController(IMediator mediator, IAuthService authService)
        {
            _mediator = mediator;
            
            _authService = authService;
        }

        public async Task<IActionResult> Wallets(int id, int pageId = 1, int take = 20,
            OrderingWalletSearch orderBy = OrderingWalletSearch.بر_اساس_تاریخ_از_آخر,
            WalletTypeSearch type = WalletTypeSearch.همه,
            WalletWhySerch why = WalletWhySerch.همه)
        {

            if (id == 0)
                id = _authService.GetLoginUserId();

            var model = await  _mediator.Send(new GetUserWalletsForAdminQuery(pageId, id, take, orderBy, type, why));
            return View(model.Value);
        }
        public IActionResult Create(int id)
        {
            var model = new CreateWallet()
            {
                UserId = id
            };
            return PartialView("Create", model);
        }
        //[HttpPost]
        //public async Task<JsonResult> Create(int id, CreateWallet model)
        //{
        //    var result = await _mediator.Send(new DepositByAdminCommand(model));

        //    return new JsonResult(result.Value);
        //}

        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateWallet model) // 2. نوع بازگشتی را به IActionResult تغییر دهید
        {
            var result = await _mediator.Send(new DepositByAdminCommand(model));

            // 3. آبجکت پاسخ را (چه خطا چه موفقیت) آماده کنید
            object responseModel;
            if (result.IsError)
            {
                responseModel = new { Success = false, Message = result.FirstError.Description };
            }
            else
            {
                responseModel = new { Success = true, Message = "عملیات موفقیت آمیز بود ." };
            }

            // 4. آبجکت را *دستی* به رشته JSON تبدیل کنید
            // (نکته: PropertyNamingPolicy = null باعث می‌شود "Success" به "success" تبدیل نشود)
            var jsonString = JsonSerializer.Serialize(responseModel, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });

            // 5. رشته را به عنوان "text/plain" برگردانید
            // جاوا اسکریپت شما حالا می‌تواند این رشته را JSON.parse کند
            return Content(jsonString, "text/plain");
        }



        public async Task<IActionResult> Transaction(int userId, int take = 20, int pageId = 1,string filter = "", OrderingWalletSearch orderBy = OrderingWalletSearch.بر_اساس_تاریخ_از_آخر,TransactionForSearch transactionFor = TransactionForSearch.همه,
            TransactionStatusSearch status = TransactionStatusSearch.همه,
            TransactionPortalSearch portal = TransactionPortalSearch.همه)
        {
            var model = await _mediator.Send(new GetTransactionsForAdminQuery(pageId, userId, filter, take, orderBy, transactionFor, status, portal)) ;
            return View(model.Value);
        }
    }
}
