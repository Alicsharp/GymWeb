using Utility.Domain;

namespace Gtm.Domain.PostDomain.UserPostAgg
{
    public class UserPost : BaseEntityCreate<int>
    {
        public UserPost(int userId, int count, string apiCode)
        {
            UserId = userId;
            Count = count;
            ApiCode = apiCode;
        }
        public void UseApi()
        {
            Count--;
        }
        public void CountPlus(int count)
        {
            Count = Count + count;
        }
        public int UserId { get; private set; } //شناسه کاربر
        public int Count { get; private set; }//اعتبار یا سهمیه کاربر یا تعدا د ر خواست
        public string ApiCode { get; private set; }
    }
}
