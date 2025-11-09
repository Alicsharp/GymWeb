using ErrorOr;
using Gtm.Domain.ShopDomain.WishListAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.WishListApp.Command
{
    /// <summary>
    /// کامند برای افزودن لیستی از محصولات به لیست علاقه‌مندی‌های کاربر
    /// (با بررسی عدم وجود تکراری)
    /// </summary>
    public record AddUsersWishListCommand(int UserId, List<int> ProductIds)
        : IRequest<ErrorOr<Success>>;
    public partial class AddUsersWishListCommandHandler
    : IRequestHandler<AddUsersWishListCommand, ErrorOr<Success>>
    {
        private readonly IWishListRepository _wishListRepository;

        public AddUsersWishListCommandHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<ErrorOr<Success>> Handle(AddUsersWishListCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductIds == null || !request.ProductIds.Any())
            {
                return Result.Success; // کاری برای انجام وجود ندارد
            }

            // 1. حل مشکل N+1: واکشی *تمام* آیتم‌های موجود کاربر در *یک* کوئری
            var existingItems = await _wishListRepository.GetAllByQueryAsync(
                w => w.UserId == request.UserId,
                cancellationToken
            );

            // 2. استفاده از HashSet برای جستجوی بسیار سریع (O(1)) در حافظه
            var existingProductIds = existingItems.Select(w => w.ProductId).ToHashSet();

            // 3. پیدا کردن آیتم‌های جدیدی که باید اضافه شوند
            var itemsToAdd = new List<WishList>();

            foreach (var productId in request.ProductIds)
            {
                // 4. مقایسه در حافظه (به جای کوئری در حلقه)
                if (!existingProductIds.Contains(productId))
                {
                    itemsToAdd.Add(new WishList(productId, request.UserId));
                }
            }

            // 5. اگر آیتم جدیدی برای افزودن وجود داشت...
            if (itemsToAdd.Any())
            {
                // 6. رعایت UoW: افزودن همه به صورت دسته‌ای (Batch) به حافظه (Context)
                await _wishListRepository.AddRangeAsync(itemsToAdd);

                // 7. ذخیره نهایی (فقط یک تراکنش دیتابیس)
                if (!await _wishListRepository.SaveChangesAsync(cancellationToken))
                {
                    return Error.Failure(description: "خطا در ذخیره لیست علاقه‌مندی‌ها.");
                }
            }

            return Result.Success;
        }
    }
}
