using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.WalletServiceApp.Query
{
    public record GetUserWalletAmountQuery(int UserId) : IRequest<int>;
    public class GetUserWalletAmountQueryHandler : IRequestHandler<GetUserWalletAmountQuery, int>
    {
        private readonly IWalletRepository _walletRepository;

        public GetUserWalletAmountQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<int> Handle(GetUserWalletAmountQuery request, CancellationToken cancellationToken)
        {
            // (فرض بر اینکه متد GetWalletAmountAsync در ریپازیتوری شما وجود دارد)
             var s =await _walletRepository.GetWalletAmountAsync(request.UserId);
            return s;
        }
    }
}
