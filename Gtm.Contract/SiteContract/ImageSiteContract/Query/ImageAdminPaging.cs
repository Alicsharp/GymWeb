using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.SiteContract.ImageSiteContract.Query
{
    public class ImageAdminPaging : BasePaging
    {
        public string Filter { get; set; }
        public List<ImageSiteAdminQueryModel> Images { get; set; }
    }
}
