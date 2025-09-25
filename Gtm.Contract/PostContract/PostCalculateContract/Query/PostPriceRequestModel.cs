using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.PostContract.PostCalculateContract.Query
{
    public class PostPriceRequestModel
    {
        public int SourceCityId { get; set; }
        public int DestinationCityId { get; set; }
        public int Weight { get; set; }
    }
}
