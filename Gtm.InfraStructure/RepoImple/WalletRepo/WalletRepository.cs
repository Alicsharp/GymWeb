using ErrorOr;
using Gtm.Application.WalletServiceApp;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.UserDomain.WalletAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.WalletRepo
{
    internal class WalletRepository : Repository<Wallet,int>, IWalletRepository
    {
        private readonly GtmDbContext _context;
        public WalletRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ErrorOr<int>> DepositByUserAsync(CreateWalletWithWhy command)
        {
            var wallet = Wallet.DepositByUser(
                command.UserId,
                command.Price,
                command.Description,
                command.WalletWhy);

            await _context.Wallets.AddAsync(wallet); // استفاده مستقیم از DbContext
            await _context.SaveChangesAsync();

            return wallet.Id;
        }

        public Task<int> GetUserBalanceAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public int GetWalletAmount(int userId)
        {
            int deposits = QueryBy(w => w.UserId == userId && w.IsPay && w.Type == WalletType.واریز).Sum(w => w.Price);
            int withdraws = QueryBy(w => w.UserId == userId && w.IsPay && w.Type == WalletType.برداشت).Sum(w => w.Price);
            return deposits - withdraws;
        }
        
    }
}
