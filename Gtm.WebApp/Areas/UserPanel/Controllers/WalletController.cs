using Gtm.Application.TransactionServiceApp.Command;
using Gtm.Application.TransactionServiceApp.Query;
using Gtm.Application.UserApp.Query;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.TransactionContract.Command;
using Gtm.WebApp.Models;
using Gtm.WebApp.Utility;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;
 

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Area("UserPanel")]
    public class WalletController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private IMediator _mediator;
        private readonly SiteData _data;
        public WalletController(IAuthService authService, IMediator mediator, IOptions<SiteData> options)
        {
            _authService = authService;
            _mediator = mediator;
            _data = options.Value;
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
            var model = await _mediator.Send(new GetTransactionsForUserPanelCommand(_userId, pageId, filter));
            return View(model.Value);
        }
       
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(int price, TransactionPortal? portal)
        {
            if (price < 1000)
            {
                ModelState.AddModelError("price", "مبلغ باید حداقل ۱۰۰۰ تومان باشد.");
                return RedirectToAction("Index");
            }

            if (portal == null) portal = TransactionPortal.زرین_پال;

            _userId = _authService.GetLoginUserId();
            var user = await _mediator.Send(new GetUserInfoForPanelQuery(_userId));

            var requestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";

            var data = new ZarinPalRequestModel
            {
                amount = price,
                callback_url = $"{_data.SiteUrl.TrimEnd('/')}/UserPanel/Wallet/PaymentCallback",
                description = "شارژ کیف پول (Sandbox)",
                merchant_id = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // ← جای Merchant ID تستی خودتان
                mobile = user.Value.Mobile,
                currency = "IRT"
            };
            try
            {
                using var http = new HttpClient();
                var json = JsonSerializer.Serialize(data);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await http.PostAsync(requestUrl, content);
                var respString = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    //_logger.LogError("ZarinPal Sandbox returned non-success: {Status} - {Body}", resp.StatusCode, respString);
                    TempData["FaildCreateWallet"] = true;
                    return RedirectToAction("Index");
                }

                var response = JsonSerializer.Deserialize<ZarinPalResponseModel>(respString);
                if (response?.data != null && response.data.code == 100)
                {
                    var authority = response.data.authority;

                    // ثبت تراکنش در دیتابیس
                    var createTransaction = new CreateTransaction
                    {
                        OwnerId = 0,
                        Portal = portal.Value,
                        Price = price,
                        TransactionFor = TransactionFor.Wallet,
                        UserId = _userId,
                        Authority = authority
                    };
                    var result = await _mediator.Send(new CreateTransactionCommand(createTransaction));
                    if (!result.IsError)
                    {
                        // هدایت به درگاه پرداخت Sandbox
                        return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{authority}");
                    }

                    TempData["FaildCreateWallet"] = true;
                    return RedirectToAction("Index");
                }
                else
                {
                    //_logger.LogError("ZarinPal Sandbox returned error object: {Body}", respString);
                    TempData["FaildCreateWallet"] = true;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Exception when calling ZarinPal Sandbox");
                TempData["FaildCreateWallet"] = true;
                return RedirectToAction("Index");
            }
        }
    }
}
