namespace Gtm.Contract.PostContract.PostCalculateContract.Query
{
    public class PostPriceResponseModel
    {
        public PostPriceResponseModel(string title, string status, int price)
        {
            Title = title;
            Status = status;
            Price = price;
        }

        public string Title { get; private set; }
        public string Status { get; private set; }
        public int Price { get; private set; }
    }
}
