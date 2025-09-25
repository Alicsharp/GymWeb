using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductCategoryRelationApp
{
    public interface IProductCategoryRelationRepository : IRepository<ProductCategoryRelation, int>
    {
    }
   
}
