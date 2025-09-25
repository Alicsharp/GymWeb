using Gtm.Application.ShopApp.ProductGalleryApp;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductGalleryRepo
{
    internal class ProductGalleryRepository : Repository<ProductGallery, int>, IProductGalleryRepository
    {
        private readonly GtmDbContext _context;
        public ProductGalleryRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
