using Gtm.Application.OrderServiceApp.Command;
using Gtm.Application.OrderServiceApp.Query;
using Gtm.Application.ShopApp.ProductSellApp.Command;
using Gtm.Application.StoresServiceApp.StroreApp.Command;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Contract.StoresContract.StoreContract.Command;
using Gtm.Contract.WalletContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Utility.Appliation.Auth;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.UserPanel.Controllers.Seller
{
    [Area("UserPanel")]
    [Authorize]
    public class OrderSellerController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;

        public OrderSellerController(int userId, IAuthService authService, IMediator mediator)
        {
            _userId = userId;
            _authService = authService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int pageId = 1)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetOrderSellersForSellerQuery(_userId, pageId, 15));
            return View(model);
        }
        public async Task<IActionResult> Detail(int id)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetOrderDetailForSellerPanelQuery(id, _userId));
            if (model.Value == null) return NotFound();
            return View(model);
        }
        public async Task<bool> ChangeStatus(int id, OrderSellerStatus status)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetOrderDetailForSellerPanelQuery(id, _userId));
            var ok = await _mediator.Send(new ChangeOrderSellerStatusBySellerCommand(id, status, _userId));
            if (status == OrderSellerStatus.لغو_شده_توسط_فروشنده)
            {
                await CheckProductAmoutsAfterPaymentAsync(id, _userId);
                await _mediator.Send(new DepositForReportOrderSellerCommand(new CreateWallet
                {
                    Description = $"لغو ریز فاکتور شماره f_{model.Value.Id}",
                    Price = model.Value.PaymentPrice + model.Value.PostPrice,
                    UserId = model.Value.UserCustomerId
                }));
                await _mediator.Send(new WithdrawForReportOrderSellerCommand(new CreateWallet()
                {
                    Description = $"لغو ریز فاکتور شماره f_{model.Value.Id}",
                    Price = model.Value.PaymentPrice + model.Value.PostPrice,
                    UserId = _userId
                }));
            }
            if (ok.IsError == false)
            {
                return false;
            }
            return true;
        }
        public async Task CheckProductAmoutsAfterPaymentAsync(int orderSellerId, int userId)
        {
            var model = await _mediator.Send(new GetOrderDetailForSellerPanelQuery(orderSellerId, userId));
            CreateStore res = new()
            {
                Description = $"لغو فاکتور شماره {model.Value.Id} توسط فروشنده",
                SellerId = model.Value.SellerId,
                Products = new List<CreateStoreProduct>()
            };
            foreach (var item in model.Value.OrderItems)
            {
                CreateStoreProduct create = new()
                {
                    Count = item.Count,
                    ProductSellId = item.ProductSellId,
                    Type = StoreProductType.افزایش
                };
                res.Products.Add(create);
            }
            var result = await _mediator.Send(new CreateStoreCommand(userId, res));
            if (result.IsError == false)
            {
                await _mediator.Send(new EditProdoctSellAmountCommand(res.Products.Select(r => new EditProdoctSellAmount
                {
                    count = r.Count,
                    SellId = r.ProductSellId,
                    Type = r.Type
                }).ToList()));
            }
        }
    }
}
