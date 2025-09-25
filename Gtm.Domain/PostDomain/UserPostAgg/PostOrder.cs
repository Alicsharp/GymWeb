using Utility.Domain;
using Utility.Domain.Enums;

namespace Gtm.Domain.PostDomain.UserPostAgg
{
    public class PostOrder : BaseEntityCreate<int>
    {
        public int UserId { get; private set; } // شناسه کاربری
        public int TransactionId { get; private set; } //شناسه تراکنش پرداحت
        public int PackageId { get; private set; } //شناسه بسته سفارش داده شده
        public int Price { get; private set; } // قیمت پرداخته شده
        public PostOrderStatus Status { get; private set; } //   وضعیت سفارش  پرداخت شده یا نشده
        public Package Package { get; private set; }
        public PostOrder(int packageId, int userId, int price)
        {

            PackageId = packageId;
            UserId = userId;
            Status = PostOrderStatus.پرداخت_نشده;
            TransactionId = 0;
            Price = price;

        }
        public void Edit(int packageId, int price)
        {

            PackageId = packageId;
            Price = price;

        }
        public void SuccessPayment(int transactionId)
        {
            TransactionId = transactionId;
            Status = PostOrderStatus.پرداخت_شده;
        }
        public PostOrder()
        {
            Package = new();
        }
    }
}
