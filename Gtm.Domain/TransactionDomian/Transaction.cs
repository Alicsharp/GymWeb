using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Domain;

namespace Gtm.Domain.TransactionDomian
{
    public class Transaction : BaseEntityCreate<long>
    {
        public Transaction(int userId, int price, TransactionPortal portal, TransactionFor transactionFor, int ownerId, string authority)
        {
            UserId = userId;
            Price = price;
            RefId = ""; //کد مرجع تراکنش از درگاه
            Portal = portal;
            Status = TransactionStatus.نا_موفق;
            TransactionFor = transactionFor;
            OwnerId = ownerId;
            Authority = authority; //کد یکتای موقت که درگاه پرداخت برای هر تراکنش جدید تولید می‌کند
        }
        public void AddWalletId(int walletId)
        {
            OwnerId = walletId;
        }
        public void Payment(TransactionStatus status, string refId)
        {
            Status = status;
            RefId = refId;
        }
        public int UserId { get; private set; }
        public int Price { get; private set; }
        public string RefId { get; private set; }
        public string Authority { get; private set; }
        public TransactionPortal Portal { get; private set; }
        public TransactionStatus Status { get; private set; }
        public TransactionFor TransactionFor { get; private set; }
        public int OwnerId { get; private set; } //شناسه سفارش   -- شناسه کیف پول اگر دو کیف پول داشته باشد
    }
}
