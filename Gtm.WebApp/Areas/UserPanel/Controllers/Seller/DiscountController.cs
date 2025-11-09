using Gtm.Application.DiscountsServiceApp.ProductDiscountServiceApp.Command;
using Gtm.Application.ShopApp.ProductApp.Query;
using Gtm.Contract.DiscountsContract.ProductDiscountContract.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Areas.UserPanel.Controllers.Seller
{
    [Area("UserPanel")]
    [Authorize]
    public class DiscountController : Controller
    {
        private int _userId;
        private readonly IAuthService _authService;
        private readonly IMediator _mediator;

        public DiscountController(IAuthService authService, IMediator mediator)
        {
            _authService = authService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create(int id) => PartialView("Create", new CreateProductDiscount()
        {
            ProductSellId = id
        });
        [HttpPost]
        public async Task<IActionResult> Create(int id, CreateProductSellDiscount model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "لطفا اطلاعات را صحیح وارد کنید." });

            if (model.ProductSellId != id || model.ProductSellId < 1)
                return BadRequest(new { message = "لطفا اطلاعات را صحیح وارد کنید." });

            // گرفتن productId
            var result = await _mediator.Send(new GetProductIdByProductSellIdQuery(model.ProductSellId));
            if (result.IsError)
                return NotFound(new { message = "محصول یافت نشد." });

            var productId = result.Value;

            // ایجاد تخفیف فروش
            var createResult = await _mediator.Send(new CreateProductSellDiscountCommand(model, productId));

            if (createResult.IsError == true)
                return BadRequest(new { message = createResult.FirstError.Description });

            return Ok(new { message = "تخفیف با موفقیت ایجاد شد." });
        }

    }
}

