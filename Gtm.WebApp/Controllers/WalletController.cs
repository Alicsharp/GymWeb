using Dto.Response.Payment;
using Gtm.Application.TransactionServiceApp.Command;
using Gtm.Application.TransactionServiceApp.Query;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.WalletContract.Command;
using Gtm.WebApp.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Utility.Domain.Enums;
using ZarinPal.Class;

namespace Gtm.WebApp.Controllers
{
    public class WalletController : Controller
    {
        private readonly SiteData _data;
        private readonly IMediator _mediator;

        public WalletController(IOptions<SiteData> option, IMediator mediator)
        {
            _data = option.Value;
            _mediator = mediator;
        }

        public async Task<IActionResult> Payment(int id, string authority, string status)
        {
            OnlinePaymentViewModel model = new()
            {
                OwnerId = 0,
                RefId = "",
                Success = false,
                TransactionFor = TransactionFor.Wallet
            };
            var transactiom = await _mediator.Send(new GetForCheckPaymentQuery(authority));
            model.Price = transactiom.Value.Price;
            model.TransactionFor = transactiom.Value.TransactionFor;
            model.OwnerId = transactiom.Value.OwnerId;
            if (transactiom.Value.Status == TransactionStatus.نا_موفق)
            {
                var payment = new Payment();
                Verification res = await payment.Verification(new Dto.Payment.DtoVerification
                {
                    Amount = transactiom.Value.Price,
                     Authority = authority,
                    //Authority = authority,
                    
                    MerchantId = _data.MerchentZarinPall
                }, ZarinPal.Class.Payment.Mode.sandbox);
                model.RefId = res.RefId.ToString();
                if (res.Status == 100)
                {
                    model.Success = true;
                    await _mediator.Send(new TransactionPaymentCommand(TransactionStatus.موفق, transactiom.Value.Id, res.RefId.ToString()));
                    switch (transactiom.Value.TransactionFor)
                    {
                        case TransactionFor.Wallet:
                            var create = new CreateWalletWithWhy(transactiom.Value.UserId, transactiom.Value.Price, "شارژ کیف پول", WalletWhy.پرداخت_از_درگاه);
                            var resCreateWallet = await _mediator.Send(new DepositByUserCommand(create));
                            await _mediator.Send(new AddTransactionWalletIdCommand(transactiom.Value.Id,resCreateWallet.Value));
                            var wallet = await _mediator.Send(new GetWalletForCheckPaymentQuery(resCreateWallet.Value));
                            model.Description = wallet.Value.Description;

                            if (wallet.Value.IsPay == false)
                            {
                                await _mediator.Send(new SuccessPaymentCommand(wallet.Value.Id));
                            }
                            break;
                        case TransactionFor.Order:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await _mediator.Send(new TransactionPaymentCommand(TransactionStatus.نا_موفق, transactiom.Value.Id, res.RefId.ToString()));
                    switch (transactiom.Value.TransactionFor)
                    {
                        case TransactionFor.Wallet:

                            model.Description = "شارژ کیف پول";
                            break;
                        case TransactionFor.Order:
                            break;
                        default:
                            break;
                    }
                }
            }

            return View(model);
        }
    }
}
