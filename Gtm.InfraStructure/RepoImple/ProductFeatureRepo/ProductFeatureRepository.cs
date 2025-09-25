using Gtm.Application.ShopApp.ProductFeatureApp;
using Gtm.Domain.ShopDomain.ProductFeaureDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductFeatureRepo
{
    internal class ProductFeatureRepository : Repository<ProductFeature,int>, IProductFeatureRepository
    {
        private readonly GtmDbContext _context;
        public ProductFeatureRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
