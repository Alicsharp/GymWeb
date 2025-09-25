using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.WalletContract.Query;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.WalletServiceApp.Query
{
    public record GetUserWalletsForAdminQuery(int pageId, int userId, int take,OrderingWalletSearch orderBy, WalletTypeSearch type, WalletWhySerch walletWhy):IRequest<ErrorOr<UserWalletForAdminPaging>>;
    public class GetUserWalletsForAdminQueryHandler : IRequestHandler<GetUserWalletsForAdminQuery, ErrorOr<UserWalletForAdminPaging>>
    {
        private readonly IUserRepo _userRepository;
        private readonly IWalletRepository _walletRepository;

        public GetUserWalletsForAdminQueryHandler(IUserRepo userRepository, IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
        }

        public async Task<ErrorOr<UserWalletForAdminPaging>> Handle(GetUserWalletsForAdminQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.userId);
            var result = _walletRepository.QueryBy(w => w.UserId == request.userId && w.IsPay);
            switch (request.type)
            {
                case WalletTypeSearch.برداشت:
                    result = result.Where(r => r.Type == WalletType.برداشت);
                    break;
                case WalletTypeSearch.واریز:
                    result = result.Where(r => r.Type == WalletType.واریز);
                    break;
                default:
                    break;
            }
            switch (request.walletWhy)
            {
                case WalletWhySerch.توسط_ادمین:
                    result = result.Where(r => r.WalletWhy == WalletWhy.توسط_ادمین);
                    break;
                case WalletWhySerch.پرداخت_از_درگاه:
                    result = result.Where(r => r.WalletWhy == WalletWhy.پرداخت_از_درگاه);
                    break;
                case WalletWhySerch.خرید_از_سایت:
                    result = result.Where(r => r.WalletWhy == WalletWhy.خرید_از_سایت);
                    break;
                case WalletWhySerch.بازگشت_فاکتور:
                    result = result.Where(r => r.WalletWhy == WalletWhy.بازگشت_فاکتور);
                    break;
                case WalletWhySerch.کارت_هدیه:
                    break;
                default:
                    break;
            }
            switch (request.orderBy)
            {
                case OrderingWalletSearch.بر_اساس_تاریخ_از_آخر:
                    result = result.OrderByDescending(r => r.CreateDate);
                    break;
                case OrderingWalletSearch.بر_اساس_تاریخ_از_اول:
                    result = result.OrderBy(r => r.CreateDate);
                    break;
                case OrderingWalletSearch.بر_اساس_مبلغ_از_بالا_به_پایین:
                    result = result.OrderByDescending(r => r.Price);
                    break;
                case OrderingWalletSearch.بر_اساس_مبلغ_از_پایین_به_بالا:
                    result = result.OrderBy(r => r.Price);
                    break;
                default:
                    break;
            }
            UserWalletForAdminPaging model = new();
            model.GetData(result, request.pageId, request.take, 2);
            model.UserId = request.userId;
            model.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
            model.WalletAmount = 0;
            model.OrderBy = request.orderBy;
            model.WalletWhy = request.walletWhy;
            model.Type = request.type;
            model.Wallets = new();
            if (model.DataCount > 0)
                model.Wallets = result.Skip(model.Skip).Take(model.Take)
                    .Select(w => new UserWalletAdminQueryModel
                    {
                        CreationDate = w.CreateDate.ToPersainDate(),
                        Description = w.Description,
                        Id = w.Id,
                        IsPay = w.IsPay,
                        Price = w.Price,
                        Type = w.Type,
                        WalletWhy = w.WalletWhy
                    }).ToList();

            model.WalletAmount = _walletRepository.GetWalletAmount(user.Id);
            return model;
        }
    }
}
