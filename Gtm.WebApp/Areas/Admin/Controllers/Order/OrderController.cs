using ErrorOr;
using Gtm.Application.AdminDashbord.Query;
using Gtm.Application.OrderServiceApp.Command;
using Gtm.Application.ShopApp.ProductApp.Command;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Application.StoresServiceApp.StroreApp.Command;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Contract.StoresContract.StoreContract.Command;
using Gtm.Contract.WalletContract.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.Admin.Controllers.Order
{
     
    [Area("Admin")]
    //[PermissionChecker(Shared.Domain.Enum.UserPermission.مدیریت_فروش)]
    public class OrderController : Controller
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int pageId = 1, int take = 15, int orderId = 0, int userId = 0, OrderAdminStatus status = OrderAdminStatus.همه)
        {
            var model = await _mediator.Send(new GetOrdersForAdminQuery(pageId, take, orderId, userId, status));
            return View(model.Value);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var model = await _mediator.Send(new GetOrderDetailForAdminQuery(id));
            if (  model.Value==null) return NotFound();
            return View(model.Value);
        }
        public async Task<bool> Cancel(int id)
        {
            try
            {
                var ok = await _mediator.Send(new CancelOrderByAdminCommand(id));
                if (ok.IsError==false)
                {
                    var model = await _mediator.Send(new GetOrderDetailForAdminQuery(id));
                    foreach (var item in model.Value.OrderSellers)
                    {
                        if (item.Status != OrderSellerStatus.لغو_شده_توسط_مشتری && item.Status != OrderSellerStatus.لغو_شده_توسط_فروشنده)
                        {
                            var userId = await _mediator.Send(new GetSellerUserIdByIdQuery(item.SellerId));
                            await CheckProductAmoutsAfterPaymentAsync(item.Id, userId.Value);
                            var res1 = await _mediator.Send(new DepositForReportOrderSellerCommand(new CreateWallet()
                            {
                                Description = $"لغو فاکتور شماره f_{model.Value.Id}",
                                Price = item.PaymentPrice + item.PostPrice,
                                UserId = userId.Value
                            }));
                            var res = await  _mediator.Send(new WithdrawForReportOrderSellerCommand(new CreateWallet()
                            {
                                Description = $"لغو فاکتور شماره f_{model.Value.Id}",
                                Price = item.PaymentPrice + item.PostPrice,
                                UserId = userId.Value
                            }));
                        }
                    }
                    var res2 = await _mediator.Send(new CancelOrderSellersCommand(id));
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private async Task CheckProductAmoutsAfterPaymentAsync(int orderSellerId, int userId)
        {
            var model = await _mediator.Send(new GetOrderSellerDetailForSellerPanelQuery(orderSellerId, userId));
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
            if (result.IsError==false)
            {
                await _mediator.Send(new EditProductSellAmountCommand(res.Products.Select(r => new EditProdoctSellAmount
                {
                    count = r.Count,
                    SellId = r.ProductSellId,
                    Type = r.Type
                }).ToList()));
            }
        }
        public async Task<bool> ImperfectOrderAsync(int id)
        {
            var result = await _mediator.Send(new ImperfectOrderCommand(id));

            // اگر خطایی وجود داشته باشد false برمی‌گرداند، در غیر این صورت true
            return !result.IsError;
        }

        public async Task<bool> Send(int id)
        {
            
            var result = await _mediator.Send(new SendOrderCommand(id));

            // اگر خطایی وجود داشته باشد false برمی‌گرداند، در غیر این صورت true
            return !result.IsError;
        }
    }
}
