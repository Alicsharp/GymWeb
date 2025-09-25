using Gtm.Application.ShopApp.ProductCategoryRelationApp;
using Gtm.Domain.ShopDomain.ProductCategoryRelationDomain;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductCategoryRelationRepo
{
    internal class ProductCategoryRelationRepository : Repository<ProductCategoryRelation,int>, IProductCategoryRelationRepository
    {
        private readonly GtmDbContext _context;
        public ProductCategoryRelationRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
