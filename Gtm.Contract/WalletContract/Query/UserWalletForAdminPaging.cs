using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.WalletContract.Query
{
    public class UserWalletForAdminPaging : BasePaging
    {
        public int UserId { get; set; }
        public int WalletAmount { get; set; }
        public string UserName { get; set; }
        public OrderingWalletSearch OrderBy { get; set; }
        public WalletWhySerch WalletWhy { get; set; }
        public WalletTypeSearch Type { get; set; }
        public List<UserWalletAdminQueryModel> Wallets { get; set; }
    }

}
