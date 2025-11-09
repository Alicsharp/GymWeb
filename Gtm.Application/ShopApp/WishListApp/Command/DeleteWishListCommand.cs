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
    public record DeleteWishListCommand(List<WishList> Wishes) : IRequest<ErrorOr<Success>>;
    public class DeleteWishListCommandHandler
    : IRequestHandler<DeleteWishListCommand, ErrorOr<Success>>
    {
        private readonly IWishListRepository _wishListRepository;
        // (فرض می‌کنیم IWishListRepository از IRepository<WishList, int> ارث می‌برد)

        public DeleteWishListCommandHandler(IWishListRepository wishListRepository)
        {
            _wishListRepository = wishListRepository;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteWishListCommand request, CancellationToken cancellationToken)
        {
            // 1. آیتم‌ها را فقط در حافظه (Context) برای حذف علامت‌گذاری کن
            _wishListRepository.RemoveRange(request.Wishes);

            // 2. هندلر مسئول ذخیره نهایی است (Unit of Work)
            if (await _wishListRepository.SaveChangesAsync(cancellationToken))
            {
                return Result.Success;
            }

            return Error.Failure(description: "خطا در حذف لیست علاقه‌مندی‌ها.");
        }
    }
}
