using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.SellerContract.Command
{
    public class EditSellerRequest : RequestSeller
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
        public string ImageAcceptName { get; set; }
    }

}
