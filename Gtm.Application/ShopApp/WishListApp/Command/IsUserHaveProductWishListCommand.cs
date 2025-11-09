using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.WishListApp.Command
{
    public record IsUserHaveProductWishListCommand(int UserId, int ProductId)
    : IRequest<ErrorOr<bool>>;
    public partial class IsUserHaveProductWishListCommandHandler
    : IRequestHandler<IsUserHaveProductWishListCommand, ErrorOr<bool>>
    {
        private readonly IWishListRepository _wishListRepository;

        public IsUserHaveProductWishListCommandHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<ErrorOr<bool>> Handle(IsUserHaveProductWishListCommand request, CancellationToken cancellationToken)
        {
            // 1. استفاده از متد ExistsAsync که در IRepository شما تعریف شده است
            bool exists = await _wishListRepository.ExistsAsync(
                c => c.ProductId == request.ProductId && c.UserId == request.UserId
            );
            return exists;
        }
    }
}
