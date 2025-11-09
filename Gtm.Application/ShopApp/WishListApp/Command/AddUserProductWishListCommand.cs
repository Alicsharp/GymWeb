using ErrorOr;
using Gtm.Domain.ShopDomain.WishListAgg;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gtm.Application.ShopApp.WishListApp.Command
{

    /// <summary>
    /// کامند برای افزودن یا حذف (Toggle) یک محصول از لیست علاقه‌مندی‌های کاربر
    /// </summary>
    /// <returns>Boolean (True = اکنون در لیست است, False = اکنون حذف شده است)</returns>
    public record AddUserProductWishListCommand(int UserId, int ProductId): IRequest<ErrorOr<Success>>;
    public class AddUserProductWishListCommandHandler
    : IRequestHandler<AddUserProductWishListCommand, ErrorOr<Success>>
    {
        private readonly IWishListRepository _wishListRepository;

        public AddUserProductWishListCommandHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<ErrorOr<Success>> Handle(AddUserProductWishListCommand request, CancellationToken cancellationToken)
        {
            // 1. حل مشکل N+1: واکشی آیتم فقط با *یک* کوئری
            // (استفاده از QueryBy() از IRepository عمومی شما)
            var wish = await _wishListRepository.QueryBy(
                    w => w.ProductId == request.ProductId && w.UserId == request.UserId
                )
                .SingleOrDefaultAsync(cancellationToken);

            if (wish != null)
            {
                // 2. اگر وجود داشت: آن را حذف کن (در حافظه)
                _wishListRepository.Remove(wish);
            }
            else
            {
                // 3. اگر وجود نداشت: آن را ایجاد کن (در حافظه)
                WishList newWish = new WishList(request.ProductId, request.UserId);
                await _wishListRepository.AddAsync(newWish);
            }

            // 4. رعایت UoW: هندلر مسئول ذخیره نهایی است
            if (await _wishListRepository.SaveChangesAsync(cancellationToken))
            {
                return Result.Success;
            }

            return Error.Failure(description: "خطا در ذخیره تغییرات لیست علاقه‌مندی‌ها.");
        }
    }

}
