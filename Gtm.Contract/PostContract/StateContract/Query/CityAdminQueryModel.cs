using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.PostContract.StateContract.Query
{
    public class CityAdminQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreationDate { get; set; }
        public CityStatus Status { get; set; }
    }
}
