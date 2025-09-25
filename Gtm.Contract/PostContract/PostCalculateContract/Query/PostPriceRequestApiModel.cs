namespace Gtm.Contract.PostContract.PostCalculateContract.Query
{
    public class PostPriceRequestApiModel
    {
        public string ApiCode { get; set; }
        public int SourceCityId { get; set; }
        public int DestinationCityId { get; set; }
        public int Weight { get; set; }
    }
}
