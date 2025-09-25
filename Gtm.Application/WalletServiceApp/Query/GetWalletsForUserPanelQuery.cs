using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.UserDomain.UserDm;
using Gtm.Domain.UserDomain.WalletAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.WalletServiceApp.Query
{
    public record GetWalletsForUserPanelQuery(int userId, int pageId, string filter):IRequest<ErrorOr<WalletUserPanelPaging>>;
    public class GetWalletsForUserPanelQueryHandelr : IRequestHandler<GetWalletsForUserPanelQuery, ErrorOr<WalletUserPanelPaging>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepo _userRepo;

        public GetWalletsForUserPanelQueryHandelr(IWalletRepository walletRepository, IUserRepo userRepo)
        {
            _walletRepository = walletRepository;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<WalletUserPanelPaging>> Handle(GetWalletsForUserPanelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Wallet> result = _walletRepository.QueryBy(w => w.UserId == request.userId && w.IsPay).OrderByDescending(w => w.Id);
            if (!string.IsNullOrEmpty(request.filter))
                result = result.Where(r => r.Description.Contains(request.filter)).OrderByDescending(w => w.Id);

            WalletUserPanelPaging model = new WalletUserPanelPaging();
            model.GetData(result, request.pageId, 15, 2);
            model.Filter = request.filter;
            model.FullName = "";
            model.UserId = request.userId;
            model.WalletAmount = 0;
            model.Wallets = new();
            if (result.Count() > 0)
                model.Wallets = result.Skip(model.Skip).Take(model.Take)
                    .Select(w => new WalletUserPanelQueryModel
                    {
                        CreationDate = w.CreateDate.ToPersainDate(),
                        Description = w.Description,
                        Id = w.Id,
                        Price = w.Price,
                        Type = w.Type
                    }).ToList();

            model.WalletAmount = _walletRepository.GetWalletAmount(model.UserId);
            var user = await _userRepo.GetByIdAsync(request.userId);
            model.FullName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
            return model;
        }
    }
}
