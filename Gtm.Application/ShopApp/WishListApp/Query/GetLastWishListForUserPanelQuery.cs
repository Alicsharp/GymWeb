using ErrorOr;
using Gtm.Contract.WishListContrct;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.WishListApp.Query
{
    /// <summary>
    /// کوئری برای دریافت ۴ آیتم آخر لیست علاقه‌مندی‌های کاربر
    /// </summary>
    public record GetLastWishListForUserPanelQuery(int UserId)
        : IRequest<ErrorOr<List<WishListForUserPanelQueryModel>>>;
    public class GetLastWishListForUserPanelQueryHandler
    : IRequestHandler<GetLastWishListForUserPanelQuery, ErrorOr<List<WishListForUserPanelQueryModel>>>
    {
        private readonly IWishListRepository _wishListRepository;
        // (فرض می‌کنیم FileDirectories یک کلاس static است)

        public GetLastWishListForUserPanelQueryHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<ErrorOr<List<WishListForUserPanelQueryModel>>> Handle(
            GetLastWishListForUserPanelQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی بهینه داده‌ها از ریپازیتوری
            var wishLists = await _wishListRepository.GetLast4WishListItemsAsync(request.UserId);

            // 2. اگر لیستی وجود نداشت، یک لیست خالی (موفقیت‌آمیز) برمی‌گردانیم
            if (wishLists == null || !wishLists.Any())
            {
                return new List<WishListForUserPanelQueryModel>();
            }

            // 3. اجرای منطق مپینگ (دقیقاً مانند کد اصلی شما)
            var model = wishLists.Select(w => new WishListForUserPanelQueryModel
            {
                Amount = w.Product.ProductSells.Sum(s => s.Amount),
                ImageAddress = FileDirectories.ProductImageDirectory100 + w.Product.ImageName,
                ImageAlt = w.Product.ImageAlt,
                Id = w.Id,
                ProductId = w.ProductId,
                Slug = w.Product.Slug,
                Title = w.Product.Title // (اصلاح شده: کد شما به اشتباه Slug را در Title می‌ریخت)
            }).ToList();

            // 4. بازگرداندن نتیجه
            return model;
        }
    }
}
