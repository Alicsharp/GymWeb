using Gtm.Domain.TransactionDomian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.TransactionServiceApp
{
    public interface ITransactionRepository : IRepository<Transaction,long>
    {
        Task<long> CreateAsyncReturnKey(Transaction transaction);
        Task<Transaction> GetByAuthorityAsync(string authority);
    };
}
