using Gtm.Application.ShopApp.ProductGalleryApp;
using Gtm.Contract.ProductGalleryContract.Query;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<GalleryForProductSingleQueryModel>> GetProductSingleGalleryAsync(int productId)
        {
            return await _context.ProductGalleries
                .Where(c => c.ProductId == productId)
                .Select(p => new GalleryForProductSingleQueryModel
                {
                    ImageAlt = p.ImageAlt,
                    ImageName = p.ImageName
                })
                .ToListAsync();
        }

    }
}
