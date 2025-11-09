using ErrorOr;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Contract.ProductSellContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Command
{
    /// <summary>
    /// کامند برای ویرایش (کاهش/افزایش) موجودی انبار لیستی از محصولات
    /// </summary>
    public record EditProductSellAmountCommand(List<EditProdoctSellAmount> Sels)
        : IRequest<ErrorOr<Success>>;
    public class EditProductSellAmountCommandHandler
    : IRequestHandler<EditProductSellAmountCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;
        // (فرض می‌کنیم IProductSellRepository از IRepository<ProductSell, int> ارث می‌برد
        // و متد GetAllByQueryAsync را دارد)

        public EditProductSellAmountCommandHandler(IProductSellRepository productSellRepository)
        {
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<Success>> Handle(EditProductSellAmountCommand request, CancellationToken cancellationToken)
        {
            // 1. استخراج تمام شناسه‌ها
            var sellIds = request.Sels.Select(s => s.SellId).Distinct().ToList();

            // 2. واکشی تمام انتیتی‌ها فقط با *یک* کوئری (حل مشکل N+1)
            var sells = await _productSellRepository.GetAllByQueryAsync(
                s => sellIds.Contains(s.Id),
                cancellationToken
            );

            // 3. تبدیل به دیکشنری برای دسترسی سریع (O(1))
            var sellsDictionary = sells.ToDictionary(s => s.Id);

            // 4. اعتبارسنجی و اعمال تغییرات در حافظه
            foreach (var item in request.Sels)
            {
                if (sellsDictionary.TryGetValue(item.SellId, out var sell))
                {
                    // انتیتی در حافظه تغییر می‌کند
                    sell.ChangeAmount(item.count, item.Type);
                    // (اگر ریپازیتوری شما به Update صریح نیاز دارد، اینجا صدا بزنید)
                    // _productSellRepository.Update(sell); 
                }
                else
                {
                    // 5. مدیریت خطا (حل مشکل NullReferenceException)
                    return Error.NotFound(description: $"ProductSell با شناسه {item.SellId} یافت نشد.");
                }
            }

            // 6. ذخیره تمام تغییرات در *یک* تراکنش
            if (await _productSellRepository.SaveChangesAsync())
            {
                return Result.Success;
            }

            return Error.Failure(description: "خطا در ذخیره‌سازی تغییرات موجودی.");
        }
    }
}
