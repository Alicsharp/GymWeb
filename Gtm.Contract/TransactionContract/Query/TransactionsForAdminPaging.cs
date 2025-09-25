using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.TransactionContract.Query
{
    public class TransactionsForAdminPaging : BasePaging
    {
        public int UserId { get; set; }
        public string PageTitle { get; set; }
        public string Filter { get; set; }
        public int TransactiionSuccessSum { get; set; }
        public OrderingWalletSearch OrderBy { get; set; }
        public TransactionPortalSearch Portal { get; set; }
        public TransactionForSearch TransactionFor { get; set; }
        public TransactionStatusSearch Status { get; set; }
        public List<TransactionForAdminQueryModel> Transactions { get; set; }
    }

}
