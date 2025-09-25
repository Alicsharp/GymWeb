using Dto.Payment;
using Gtm.Application.TransactionServiceApp.Command;
using Gtm.Application.TransactionServiceApp.Query;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.TransactionContract.Command;
using Gtm.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;
using ZarinPal.Class;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
   
    [Area("UserPanel")]
    public class WalletController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private IMediator _mediator;
        private readonly SiteData _data;


        public WalletController(IAuthService authService, IMediator mediator,IOptions<SiteData> options)
        {
            _authService = authService;
            _mediator = mediator;   
            _data= options.Value;   
        }
       

        public async Task<IActionResult> Index(int pageId, string filter)
        {

            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetWalletsForUserPanelQuery(_userId, pageId, filter));
            return View(model.Value);
        }
        public async Task<IActionResult> Transactions(int pageId, string filter)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetTransactionsForUserPanelCommand(_userId, pageId, filter)) ;
            return View(model.Value);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(int price, TransactionPortal? portal)
        {

            if (portal == null)
                portal = TransactionPortal.زرین_پال;
            if (price < 1000)
                return NotFound();
            _userId = _authService.GetLoginUserId();

            CreateTransaction createTransaction = new CreateTransaction()
            {
                OwnerId = 0,
                Portal = portal.Value,
                Price = price,
                TransactionFor = TransactionFor.Wallet,
                UserId = _userId
            };
            var result = await _mediator.Send(new CreateTransactionCommand(createTransaction));
            if (result.IsError == false)
            {
                switch (portal)
                {
                    case TransactionPortal.زرین_پال:
                        var payment = new ZarinPal.Class.Payment();
                        //var resultPayment = await payment.Request(new DtoRequest
                        //{
                        //    //Amount = price,
                        //    //CallbackUrl = $"{_data.SiteUrl}Wallet/Payment/{result.Value}",
                        //    //Description = "شارژ کیف پول",
                        //    //MerchantId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",

                        //}, Payment.Mode.sandbox);
                        var resultPayment = await payment.Request(new DtoRequest
                        {
                            Amount = price,
                            CallbackUrl = $"{_data.SiteUrl}Wallet/Payment/{result.Value}",
                            Description = "شارژ کیف پول",
                            MerchantId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",

                        },Payment.Mode.zarinpal);
                        if (resultPayment.Status == 100)
                  
                            return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{resultPayment.Authority}");
                        break;
                }
                TempData["FaildCreateWallet"] = true;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["FaildCreateWallet"] = true;
                return RedirectToAction("Index");
            }

        }
    }
}
 
