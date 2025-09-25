using Gtm.Application.TransactionServiceApp.Query;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.WalletContract.Command;
using Gtm.WebApp.Utility;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.User
{
    [Area("Admin")]
    //[PermissionChecker (UserPermission.کیف_پول)]
    public class WalletController : Controller
    {
        private readonly IMediator _mediator;

        public WalletController(IMediator mediator)
        {
            _mediator = mediator;
        }
 
        public async Task<IActionResult> Wallets(int id, int pageId = 1, int take = 20,
            OrderingWalletSearch orderBy = OrderingWalletSearch.بر_اساس_تاریخ_از_آخر,WalletTypeSearch type = WalletTypeSearch.همه, WalletWhySerch why = WalletWhySerch.همه)
        {
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
        [HttpPost]
        public async Task<JsonResult> Create(int id, CreateWallet model)
        {
            if (id != model.UserId)
                return new JsonResult(new { Success = false, Message = "لطفا اطلاعات را درست وارد کنید." });

            if (model.Price < 1000)
                return new JsonResult(new { Success = false, Message = "مبلغ باید بیشتر از 1,000 تومان باشد." });

            var result = await _mediator.Send(new DepositByAdminCommand(model));

            if (result.IsError)
            {
                // می‌تونی همه ارورها رو یکجا برگردونی
                return new JsonResult(new
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return new JsonResult(new { Success = true, Message = "واریز با موفقیت انجام شد." });
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
