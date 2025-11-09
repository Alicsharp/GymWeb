using ErrorOr;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Command;
using Gtm.Application.DiscountsServiceApp.OrderDiscountsServiceApp.Query;
using Gtm.Application.OrderServiceApp.Command;
using Gtm.Application.OrderServiceApp.Query;
using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Application.PostServiceApp.CityApp.Query;
using Gtm.Application.PostServiceApp.PostCalculateApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Command;
using Gtm.Application.ShopApp.ProductApp.Command;
using Gtm.Application.ShopApp.SellerApp.Command;
using Gtm.Application.ShopApp.SellerApp.Query;
using Gtm.Application.StoresServiceApp.StroreApp.Command;
using Gtm.Application.TransactionServiceApp.Command;
using Gtm.Application.UserAddressApp.Command;
using Gtm.Application.UserAddressApp.Query;
using Gtm.Application.UserApp.Query;
using Gtm.Application.WalletServiceApp.Command;
using Gtm.Application.WalletServiceApp.Query;
using Gtm.Contract.DiscountsContract.OrderDiscountContract.Command;
using Gtm.Contract.OrderAddressContract.Command;
using Gtm.Contract.OrderContract.Command;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Contract.PostContract.PostPriceContract.Query;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Contract.StoresContract.StoreContract.Command;
using Gtm.Contract.TransactionContract.Command;
using Gtm.Contract.UserAddressContract.Command;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.ShopDomain.OrderDomain.OrderSellerDomain;
using Gtm.WebApp.Models;
using Gtm.WebApp.Utility;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility.Appliation.Auth;
using Utility.Contract;
using Utility.Domain.Enums;

namespace Gtm.WebApp.Areas.UserPanel.Controllers
{
    [Area("UserPanel")]
    [Authorize]
    public class OrderController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;
        private readonly SiteData _data;


        public OrderController(IAuthService authService, IMediator mediator, IOptions<SiteData> siteDataOptions)
        {
            _authService = authService;
            _mediator = mediator;

            // 3. برای دسترسی به آبجکت، از .Value استفاده کنید
            _data = siteDataOptions.Value;
        }
        public async Task<IActionResult> Order()
        {
            _userId = _authService.GetLoginUserId();
            //List<ShopCartViewModel> model = new();
            //string cookieName = "boloorShop-cart-items";
            //if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
            //{
            //    model = JsonSerializer.Deserialize<List<ShopCartViewModel>>(cartJson);
            //    var ok = await _mediator.Send(new UbsertOpenOrderForUserCommand(_userId, model));
            //    Response.Cookies.Delete(cookieName);
            //}
            await _mediator.Send(new CheckOrderItemDataCommand(_userId));
            await _mediator.Send(new CheckOrderEmptyCommand(_userId));
            var res = await _mediator.Send(new GetOpenOrderForUserQuery(_userId));
            if (res.Value == null)
            {
                TempData["noOpenOrder"] = true;
                return Redirect("/Shop");
            }
            return View(res.Value);
        }
        public async Task<bool> DeleteOrderItem(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new DeleteOrderItemCommand(id, _userId));
            if (res.IsError)
                return false;
            return true;
        }
        public async Task<IActionResult> OrderItemMinus(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new OrderItemMinusCommand(id, _userId));

            var model = new
            {
                Success = !res.IsError,
                Message = res.IsError
                    ? string.Join(" , ", res.Errors.Select(e => e.Description))
                    : "محصول با موفقیت کم شد"
            };

            return Json(model); // ❌ بدون Serialize دستی
        }


        public async Task<IActionResult> OrderItemPlus(int id)
        {
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new OrderItemPlusCommand(id, _userId));

            var model = new
            {
                success = !res.IsError,
                message = res.IsError
                    ? string.Join(" , ", res.Errors.Select(e => e.Description))
                    : "محصول با موفقیت اضافه شد"
            };

            return Json(model); // ❌ بدون Serialize
        }

        [HttpPost]
        public async Task<JsonResult> AddOrderSellerDiscount(int id, string code)
        {
            var userId = _authService.GetLoginUserId();

            // بررسی اینکه کاربر سفارش باز برای این فروشنده دارد یا خیر
            var orderResult = await _mediator.Send(new HaveUserOpenOrderSellerAsyncByOrderSellerIdQuery(userId, id));
            if (orderResult.IsError)
            {
                return Json(new { Success = false, Message = orderResult.FirstError.Description });
            }

            // اینجا باید سرویس تخفیف رو صدا بزنی تا اطلاعات کد تخفیف گرفته بشه
            // فرض می‌کنیم متدی داری که بر اساس code جزئیات تخفیف رو برمی‌گردونه
            var discountResult = await _mediator.Send(new GetDiscountByCodeQuery(id, code));
            if (discountResult.IsError)
            {
                return Json(new { Success = false, Message = discountResult.FirstError.Description });
            }

            var discount = discountResult.Value;

            // اضافه کردن تخفیف به سفارش
            var addDiscountResult = await _mediator.Send(
                new AddOrderSellerDiscountCommand(userId, id, discount.Id, discount.Title, discount.Percent)
            );

            if (addDiscountResult.IsError)
            {
                return Json(new { Success = false, Message = addDiscountResult.FirstError.Description });
            }

            // کم کردن یک استفاده از تخفیف
            var minusResult = await _mediator.Send(new MinusUseOrderDiscountCommand(discount.Id));
            if (minusResult.IsError)
            {
                return Json(new { Success = false, Message = minusResult.FirstError.Description });
            }

            return Json(new { Success = true, Message = "تخفیف با موفقیت اعمال شد." });
        }
        [HttpPost]
        public async Task<JsonResult> AddOrderDiscount(string code)
        {
            _userId = _authService.GetLoginUserId();

            // 1. بررسی وجود سبد باز
            var openOrderResult = await _mediator.Send(new HaveUserOpenOrderQuery(_userId));

            if (openOrderResult.IsError)
            {
                // خطای سیستمی
                return Json(new { Success = false, Message = openOrderResult.FirstError.Description });
            }
            if (openOrderResult.Value == false)
            {
                // خطای بیزینسی
                return Json(new { Success = false, Message = "شما فاکتور بازی ندارید ." });
            }

            // 2. اعتبارسنجی و گرفتن اطلاعات کد تخفیف
            var discountResult = await _mediator.Send(new GetOrderDiscountForAddOrderdiscountQuery(code));

            if (discountResult.IsError)
            {
                return Json(new { Success = false, Message = discountResult.FirstError.Description });
            }

            var discountData = discountResult.Value; // اطلاعات تخفیف

            // 3. اضافه کردن تخفیف به سفارش (Command)
            var addResult = await _mediator.Send(new AddOrderDiscountCommand(
                _userId,
                discountData.Id,
                discountData.Title,
                discountData.Percent));

            // 4. بررسی نتیجه نهایی
            if (addResult.IsError)
            {
                // عملیات جبرانی (Compensation): برگرداندن تعداد استفاده
                await _mediator.Send(new MinusUseOrderDiscountCommand(discountData.Id));

                return Json(new { Success = false, Message = "عملیات نا موفق !! مجددا تلاش کنید " });
            }

            // --- موفقیت کامل ---
            // پیام موفقیت را از نتیجه اعتبارسنجی تخفیف برمی‌گردانیم
            return Json(new { Success = true, Message = discountData.Message });
        }
        public async Task<JsonResult> OpenOrderItems()
        {
            _userId = _authService.GetLoginUserId();
            List<ShopCartViewModel> model = new();
            string cookieName = "boloorShop-cart-items";
            if (Request.Cookies.TryGetValue(cookieName, out var cartJson))
            {
                model = JsonSerializer.Deserialize<List<ShopCartViewModel>>(cartJson);
                var ok = await _mediator.Send(new UbsertOpenOrderForUserCommand(_userId, model));
                Response.Cookies.Delete(cookieName);
            }
            await _mediator.Send(new CheckOrderItemDataCommand(_userId));
            await _mediator.Send(new CheckOrderEmptyCommand(_userId));
            var res = await _mediator.Send(new GetOpenOrderItemsQuery(_userId));
            var json = JsonSerializer.Serialize(res);
            return Json(json);
        }
        [HttpPost]
        public async Task<JsonResult> AddOrderItem(int id)
        {
            _userId = _authService.GetLoginUserId();

            // 1. کامند AddOrderItem را ارسال می‌کنیم
            // این کامند حالا ErrorOr<Success> برمی‌گرداند
            ErrorOr<Success> res = await _mediator.Send(new AddOrderItemCommand(_userId, id));

            // 2. بررسی می‌کنیم که آیا خطا رخ داده است
            if (res.IsError==false)
            {
                // اگر خطا داشتیم، یک آبجکت ناشناس می‌سازیم و آن را Json می‌کنیم
                // پیام خطا را از خود نتیجه ErrorOr می‌خوانیم
                await _mediator.Send(new CheckOrderItemDataCommand(_userId));
            }

            // 3. اگر عملیات موفقیت‌آمیز بود (IsError == false)
            // کامند دوم را اجرا می‌کنیم
           

            // 4. یک آبجکت ناشناس برای موفقیت می‌سازیم و آن را Json می‌کنیم
            // (چون ErrorOr<Success> در حالت موفقیت پیامی ندارد،
            //  یک پیام موفقیت پیش‌فرض اینجا می‌گذاریم)
            return Json(new { Success = true, Message = "آیتم با موفقیت به سبد خرید اضافه شد." });
        }
        public async Task<IActionResult> AddOrderAddress()
        {
            _userId = _authService.GetLoginUserId();
            var ok = await _mediator.Send(new HaveUserOpenOrderQuery(_userId));
            if (ok.IsError == false)
            {
                var model = await _mediator.Send(new GetAddresseForUserPanelQuery(_userId));
                return PartialView("AddOrderAddress", model.Value);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<JsonResult> AddOrderAddress(CreateAddress model)
        {
            _userId = _authService.GetLoginUserId();
            model.UserId=_userId;
            // 1. بررسی وجود سبد باز
            // (فرض می‌کنیم این کوئری ErrorOr<bool> برمی‌گرداند)
            var openOrderResult = await _mediator.Send(new HaveUserOpenOrderQuery(_userId));

            // خطای سیستمی (مثلا خطای دیتابیس)
            if (openOrderResult.IsError)
            {
                return Json(new { Success = false, Message = openOrderResult.FirstError.Description });
            }
            // خطای بیزینسی (سبد باز ندارد)
            if (openOrderResult.Value == false)
            {
                return Json(new { Success = false, Message = "شما فاکتور بازی ندارید ." });
            }

            // 2. اعتبارسنجی استان
            // (فرض می‌کنیم این کامند در صورت خطا، ErrorOr<Success> با خطا برمی‌گرداند)
            var stateResult = await _mediator.Send(new IsStateCorrectCommand(model.StateId));
            if (stateResult.IsError)
            {
                // (این منطق صحیح است، به جای IsError == false)
                return Json(new { Success = false, Message = "لطفا یک استان انتخاب کنید" });
            }

            // 3. اعتبارسنجی شهر
            var cityResult = await _mediator.Send(new IsCityCorrectCommand(model.StateId, model.CityId));
            if (cityResult.IsError)
            {
                return Json(new { Success = false, Message = "لطفا یک شهر انتخاب کنید" });
            }

            // 4. ایجاد آدرس کاربر (UserAddress)
            // (فرض می‌کنیم این کامند هم ErrorOr<Success> برمی‌گرداند)
            var userAddressResult = await _mediator.Send(new CreateUserAddressCommand(model));
            if (userAddressResult.IsError)
            {
                return Json(new { Success = false, Message = userAddressResult.FirstError.Description });
            }

            // 5. ایجاد یا به‌روزرسانی آدرس سفارش (OrderAddress)
            // (این کامند هم ErrorOr<Success> برمی‌گرداند)
            var orderAddressModel = new CreateOrderAddress
            {
                AddressDetail = model.AddressDetail,
                CityId = model.CityId,
                FullName = model.FullName,
                IranCode = model.IranCode,
                Phone = model.Phone,
                PostalCode = model.PostalCode,
                StateId = model.StateId,
            };

            var orderAddressResult = await _mediator.Send(new CreateOrUpdateOrderAddressCommand(orderAddressModel, _userId));
            if (orderAddressResult.IsError)
            {
                return Json(new { Success = false, Message = orderAddressResult.FirstError.Description });
            }

            // 6. موفقیت کامل
            // اگر کد به اینجا برسد، یعنی تمام مراحل با موفقیت انجام شده‌اند
            return Json(new { Success = true, Message = "آدرس با موفقیت ثبت شد." });
        }
        public async Task<bool> ChangeOrderAddress(int id)
        {
            _userId = _authService.GetLoginUserId();
            bool isAddressForUser = await _mediator.Send(new IsAddressForUserQuery(id, _userId));
            if (!isAddressForUser) return false;
            CreateAddress? model = await _mediator.Send(new GetAddressForAddToFactorQuery(id));
            var res = await _mediator.Send(new CreateOrderAddressCommand(new CreateOrderAddress
            {
                AddressDetail = model.AddressDetail,
                CityId = model.CityId,
                FullName = model.FullName,
                IranCode = model.IranCode,
                Phone = model.Phone,
                PostalCode = model.PostalCode,
                StateId = model.StateId,
            }, _userId));
            if (res.IsError == false) { return false; }
            return true;
        }
        public async Task<JsonResult> CalculatePostPrice(int id)
        {
            PostResposeModel model = new() { success = false, sellerId = id };
            _userId = _authService.GetLoginUserId();
            var res = await _mediator.Send(new GetOpenOrderForUserQuery(_userId));

            if (res.Value == null)
            {
                model.message = "";
            }
            else
            {
                if (res.Value.OrderAddress == null || res.Value.OrderAddress.CityId == 0)
                {
                    model.message = "برای محاسبه قیمت پست ابتدا آدرس را وارد کنید .";
                }
                else
                {
                    var orderseller = res.Value.Ordersellers.SingleOrDefault(s => s.Id == id);
                    if (orderseller == null)
                    {
                        model.message = "";
                    }
                    else
                    {
                        // دریافت شهر فروشنده
                        int sourceCity = await _mediator.Send(new GetCityOfSellerQuery(orderseller.SellerId));
                        if (sourceCity == 0)
                        {
                            model.message = "";
                        }
                        else
                        {
                            // ارسال درخواست برای گرفتن وزن
                            var weightResult = await _mediator.Send(new GetOrderSellerWeightQuery(id));

                            // بررسی نتیجه و اطمینان از اینکه وزن معتبر است
                            if (weightResult.IsError || weightResult.Value == 0)
                            {
                                model.message = "";
                            }
                            else
                            {
                                var posts = await _mediator.Send(new PostCalculateCommand(new PostPriceRequestModel
                                {
                                    DestinationCityId = res.Value.OrderAddress.CityId,
                                    SourceCityId = sourceCity,
                                    Weight = weightResult.Value // استفاده از مقدار موفقیت
                                }));

                                model.success = true;
                                model.posts = posts.Value;
                            }
                        }
                    }
                }
            }

            var json = JsonSerializer.Serialize(model);
            return Json(json);
        }

        [HttpPost]
        public async Task<bool> ChoosePostPrice(int id, int post, string postTitle)
        {
            _userId = _authService.GetLoginUserId();

            var res = await _mediator.Send(new GetOpenOrderForUserQuery(_userId));
            if (res.Value == null)
                return false;

            if (res.Value.OrderAddress == null || res.Value.OrderAddress.CityId == 0)
                return false;

            var orderSeller = res.Value.Ordersellers.SingleOrDefault(s => s.Id == id);
            if (orderSeller == null)
                return false;

            int sourceCity = await _mediator.Send(new GetCityOfSellerQuery(orderSeller.SellerId));
            if (sourceCity == 0)
                return false;

            var weight = await _mediator.Send(new GetOrderSellerWeightQuery(id));
            if (weight.Value == 0)
                return false;

            // حالت پست دستی (بدون انتخاب از لیست پست‌ها)
            if (post == 0)
            {
                if (!string.IsNullOrEmpty(postTitle))
                {
                    var result = await _mediator.Send(
                        new AddPostToOrderSellerCommand(_userId, id, 0, 0, postTitle)
                    );

                    return !result.IsError; // موفقیت = true
                }

                return false;
            }

            // محاسبه قیمت پست‌ها
            var posts = await _mediator.Send(new PostCalculateCommand(new PostPriceRequestModel
            {
                DestinationCityId = res.Value.OrderAddress.CityId,
                SourceCityId = sourceCity,
                Weight = weight.Value
            }));

            var postChoose = posts.Value.SingleOrDefault(s => s.PostId == post);
            if (postChoose == null)
                return false;

            // افزودن پست انتخاب‌شده به سفارش
            var ok = await _mediator.Send(
                new AddPostToOrderSellerCommand(
                    _userId,
                    id,
                    postChoose.PostId,
                    postChoose.Price,
                    postTitle
                )
            );

            return !ok.IsError;
        }
        [HttpPost]  
        public async Task<JsonResult> ChangePayment(OrderPayment? pay)
        {
            _userId = _authService.GetLoginUserId();

            // 1. اعتبارسنجی ورودی (به جای ساخت OperationResult)
            if (pay == null)
            {
                // مستقیماً JSON خطا را برمی‌گردانیم
                return Json(new { Success = false, Message = "لطفا یک روش پرداخت انتخاب کنید." });
            }

            // 2. ارسال کامند به MediatR
            // (این همان کامندی است که هندلر آن را در مرحله قبل ساختیم)
            var result = await _mediator.Send(new ChangeOrderPaymentCommand(_userId, pay.Value));

            // 3. بررسی نتیجه ErrorOr
            if (result.IsError)
            {
                // برگرداندن JSON خطا بر اساس پیام ErrorOr
                // (مثلاً "سبد خرید بازی یافت نشد" یا خطای ذخیره‌سازی)
                return Json(new { Success = false, Message = result.FirstError.Description });
            }

            // 4. برگرداندن JSON موفقیت‌آمیز
            return Json(new { Success = true, Message = "روش پرداخت با موفقیت تغییر کرد." });
        }
        public async Task<IActionResult> PaymentFactor()
        {
            _userId = _authService.GetLoginUserId();
            OperationResultfactor res = new(false);
            var model = await _mediator.Send(new GetOpenOrderForUserQuery(_userId));
            if (model.Value.OrderAddress == null) res.Message = "آدرس را وارد کنید ";
            else
            {
                bool ok = true;
                foreach (var item in model.Value.Ordersellers)
                    if (string.IsNullOrEmpty(item.PostTitle))
                    {
                        res.Message = "برای هر فاکتور فروشنده پست را انتخاب کنید .";
                        ok = false;
                    }
                if (ok)
                {
                    if (model.Value.OrderPayment == OrderPayment.پرداخت_از_درگاه)
                    {
                        var user = await _mediator.Send(new GetUserInfoForPanelQuery(_userId));

                        var requestZarinPalUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
                        ZarinPalRequestModel data = new ZarinPalRequestModel
                        {
                            amount = model.Value.PaymentPrice,
                            callback_url = $"{_data.SiteUrl}Wallet/Payment",
                            currency = "IRT",
                            description = "شارژ کیف پول",
                            merchant_id = _data.MerchentZarinPall,
                            mobile = user.Value.Mobile
                        };
                        using (WebClient client = new WebClient())
                        {
                            // تنظیم هدرها (اگر لازم است)
                            client.Headers[HttpRequestHeader.ContentType] = "application/json";
                            string jsonData = JsonSerializer.Serialize(data);
                            // تبدیل داده‌های JSON به بایت‌ها
                            byte[] requestData = Encoding.UTF8.GetBytes(jsonData);

                            // ارسال درخواست POST و دریافت پاسخ
                            byte[] responseData = client.UploadData(requestZarinPalUrl, "POST", requestData);

                            // تبدیل پاسخ به رشته
                            string responseString = Encoding.UTF8.GetString(responseData);
                            ZarinPalResponseModel response = JsonSerializer.Deserialize<ZarinPalResponseModel>(responseString);

                            if (response.data.code == 100 && response.data.message.ToLower() == "success")
                            {
                                CreateTransaction createTransaction = new CreateTransaction()
                                {
                                    OwnerId = model.Value.OrderId,
                                    Portal = TransactionPortal.زرین_پال,
                                    Price = model.Value.PaymentPrice,
                                    TransactionFor = TransactionFor.Order,
                                    UserId = _userId,
                                    Authority = response.data.authority
                                };
                                var result = await _mediator.Send(new CreateTransactionCommand(createTransaction));
                                res.Success = true;
                                res.Message = "در حال انتقال به درگاه ";
                                res.Url = $"https://sandbox.zarinpal.com/pg/StartPay/{response.data.authority}";
                            }
                            else
                            {

                                res.Message = "خطای درگاه پرداخت !!! پس از دقایقی مجددا تلاش کنید .";
                            }
                        }
                    }
                    else
                    {
                        int walletUserAmount = await _mediator.Send(new GetUserWalletAmountQuery(_userId ));
                        if (walletUserAmount < model.Value.PaymentPrice)
                        {
                            int price = model.Value.PaymentPrice - walletUserAmount;
                            res.Message = $"کیف پول شما مبلغ {price.ToString("#,0")}کم دارد لطفا کیف پول خور را شارژ کنید .";
                            res.Url = "/UserPanel/Wallet/Index";
                        }
                        else
                        {
                            var result = await  _mediator.Send(new WithdrawalCommand(new CreateWalletWithWhy
                            {
                                Description = $"پرداخت فاکتور شماره {model.Value.OrderId}",
                                Price = model.Value.PaymentPrice,
                                UserId = _userId,
                                WalletWhy = WalletWhy.خرید_از_سایت
                            }));
                            if (result.IsError==false)
                            {
                                var orderId = await _mediator.Send(new PaymentSuccessOrderCommand(_userId, model.Value.PaymentPrice));
                                if (orderId.Value > 0)
                                {
                                    await CheckProductAmoutsAfterPaymentAsync(orderId.Value);
                                    foreach (var item in model.Value.Ordersellers)
                                    {
                                        var userSellerId = await _mediator.Send(new GetSellerUserIdByIdQuery(item.SellerId));
                                        await _mediator.Send(new DepositForPaymentOrderSellerCommand(new CreateWallet()
                                        {
                                            Description = $"پرداخت ریز فاکتور شماره f_{item.Id}",
                                            Price = item.PaymentPrice + item.PostPrice,
                                            UserId = userSellerId.Value
                                        }));
                                    }
                                    res.Success = true;
                                    res.Message = "فاکتور با موفقیت از کیف پول شما پرداخت شد ";
                                    res.Url = $"/UserPanel/Order/Detail/{model.Value.OrderId}";
                                }
                                else
                                {
                                    res.Message = "خطای سیستم !! تماس با سایت .";
                                }
                            }
                            else
                            {
                                res.Message = "خطای سیستم !! تماس با سایت .";
                            }
                        }
                    }
                }
            }
            var json = JsonSerializer.Serialize(res);
            return Json(json);
        }
        public async Task<IActionResult> Detail(int id)
        {
            _userId = _authService.GetLoginUserId();
            var model = await _mediator.Send(new GetOrderDetailForUserPanelQuery(id, _userId));
            if (model.Value == null) return NotFound();
            if (model.Value.OrderStatus == OrderStatus.پرداخت_نشده) return RedirectToAction("Order");
            return View(model);
        }
        public async Task<IActionResult> Orders(int pageId = 1)
        {
            _userId = _authService.GetLoginUserId();
            var model =  await _mediator.Send(new GetOrdersForUserPanelQuery(_userId,pageId,15));
            return View(model);
        }
        public async Task CheckProductAmoutsAfterPaymentAsync(int orderId)
        {
            var model = await _mediator.Send(new GetPaidOrderDetailQuery(orderId));
            foreach (var orderSeller in model.Value.OrderSellers)
            {
                var userId = await _mediator.Send(new GetSellerUserIdByIdQuery(orderSeller.SellerId));
                CreateStore res = new CreateStore()
                {
                    Description = $"پرداخت فاکتور شماره {orderSeller.Id}",
                    SellerId = orderSeller.SellerId,
                    Products = new List<CreateStoreProduct>()
                };
                foreach (var item in orderSeller.OrderItems)
                {
                    CreateStoreProduct create = new CreateStoreProduct() 
                    {
                        Count = item.Count,
                        ProductSellId = item.ProductSellId,
                        Type = StoreProductType.کاهش
                    };
                    res.Products.Add(create);
                }
                var result = await _mediator.Send(new CreateStoreCommand(userId.Value,res));
                if (result.IsError==false)
                {
                    // 1. ابتدا لیست EditProdoctSellAmount را بسازید
                    var dtoList = res.Products.Select(r => new EditProdoctSellAmount
                    {
                        count = r.Count, // (این فیلد در CreateStoreProduct وجود نداشت، فرض می‌کنم منظورتان Count است)
                        SellId = r.ProductSellId,
                        Type = r.Type
                    }).ToList();

                    // 2. سپس لیست ساخته شده را به کامند پاس دهید
                    await _mediator.Send(new EditProductSellAmountCommand(dtoList));
                }
            }
        }
    }

}
