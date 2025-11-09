using Gtm.Contract.PostContract.PostCalculateContract.Query;

namespace Gtm.Contract.PostContract.PostPriceContract.Query
{
    public class PostResposeModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int sellerId { get; set; }
        public List<PostPriceResponseModel> posts { get; set; }
    }
}
