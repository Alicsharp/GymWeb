using MediatR;

namespace Gtm.Application.ShopApp.WishListApp.Command
{
    /// <summary>
    /// کوئری برای دریافت تعداد آیتم‌های موجود در لیست علاقه‌مندی‌های کاربر
    /// </summary>
    /// <param name="UserId">شناسه کاربر</param>
    public record GetUserWishListCountQuery(int UserId) : IRequest<int>;
    public class GetUserWishListCountQueryHandler
    : IRequestHandler<GetUserWishListCountQuery, int>
    {
        private readonly IWishListRepository _wishListRepository;

        public GetUserWishListCountQueryHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<int> Handle(GetUserWishListCountQuery request, CancellationToken cancellationToken)
        {
            // به جای: _wishListRepository.GetAllByQuery(...).Count();
            // از متد بهینه و غیرهمزمان CountAsync که در IRepository شما بود استفاده می‌کنیم:

            return await _wishListRepository.CountAsync(
                c => c.UserId == request.UserId
            );
        }
    }
}
