namespace Gtm.Contract.PostContract.PostCalculateContract.Query
{
    public class PostPriceResponseApiModel
    {
        public PostPriceResponseApiModel(List<PostPriceResponseModel> prices, string message, bool success)
        {
            Prices = prices;
            Message = message;
            Success = success;
        }

        public List<PostPriceResponseModel> Prices { get; private set; }
        public string Message { get; private set; }
        public bool Success { get; private set; }

    }
}
