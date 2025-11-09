using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.OrderContract.Query
{
    public class OrderUserPanelPaging : BasePaging
    {
        public List<OrderForUserPanelQueryModel> Orders { get; set; }
    }
}
