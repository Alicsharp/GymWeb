using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.WalletContract.Command
{

    public class CreateWalletWithWhy
    {
        public CreateWalletWithWhy(int userId, int price, string description, WalletWhy walletWhy)
        {
            UserId = userId;
            Price = price;
            Description = description;
            WalletWhy = walletWhy;
        }
        public CreateWalletWithWhy()
        {

        }
        public int UserId { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public WalletWhy WalletWhy { get; set; }
    }
}
