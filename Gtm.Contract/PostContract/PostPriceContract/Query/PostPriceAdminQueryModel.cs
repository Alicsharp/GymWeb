using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.PostContract.PostPriceContract.Query
{
    public class PostPriceAdminQueryModel
    {
        public int Id { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int TehranPrice { get; set; }
        public int StateCenterPrice { get; set; }
        public int CityPrice { get; set; }
        public int InsideStatePrice { get; set; }
        public int StateClosePrice { get; set; }
        public int StateNonClosePrice { get; set; }
    }
}
