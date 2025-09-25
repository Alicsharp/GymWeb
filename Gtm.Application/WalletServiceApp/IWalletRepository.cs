using ErrorOr;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.UserDomain.WalletAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.WalletServiceApp
{
    public interface IWalletRepository : IRepository<Wallet,int>
    {
        //Task<OperationResultWithKey> DepositByUserAsync(CreateWalletWithWhy command);
        Task<ErrorOr<int>> DepositByUserAsync(CreateWalletWithWhy command);
        Task<int> GetUserBalanceAsync(int userId);
        int GetWalletAmount(int userId);

    }
}
