using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.SiteContract.BanarContract.Query
{
    public class BanerForAdminQueryModel
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
        public string ImageAlt { get; set; }
        public BanerState State { get; set; }
        public bool Active { get; set; }
        public string CreationDate { get; set; }
    }
}
