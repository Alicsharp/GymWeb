using Gtm.Domain.ShopDomain.ProductFeaureDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductFeatureApp
{
    public interface IProductFeatureRepository : IRepository<ProductFeature,int>  
    {
    }
}
