using Gtm.Application.TransactionServiceApp;
using Gtm.Domain.TransactionDomian;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.TransactionRepo
{
    internal class TransactionRepository : Repository<Transaction, long>, ITransactionRepository
    {
        private readonly GtmDbContext _context;
        public TransactionRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<long> CreateAsyncReturnKey(Transaction transaction)
        {
            try
            {
                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync(); 
                return transaction.Id;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }
            catch
            {
                return 0; // یا مقدار پیش‌فرض دیگری در صورت خطا
            }
            //if (await AddAsync(transaction))
            //    return transaction.Id;
            //return 0;
        }

        public Task<Transaction> GetByAuthorityAsync(string authority) =>
            _context.Transactions.SingleOrDefaultAsync(s => s.Authority.ToLower().Trim() == authority.ToLower().Trim());
    }
}
