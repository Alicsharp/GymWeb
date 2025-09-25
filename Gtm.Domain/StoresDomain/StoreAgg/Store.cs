using Gtm.Domain.StoresDomain.StoreProductAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.StoresDomain.StoreAgg
{
    public class Store : BaseEntityCreate<int>
    {
        public int UserId { get; private set; }
        public int SellerId { get; private set; }
        public string Description { get; private set; }
        public List<StoreProduct> StoreProducts { get; private set; }

        public Store()
        {
            StoreProducts = new();
        }

        public Store(int userId, int sellerId, string description)
        {
            UserId = userId;
            SellerId = sellerId;
            Description = description;
            StoreProducts = new();
        }

        public void EditDescription(string des)
        {
            Description = des;
        }
    }

}
