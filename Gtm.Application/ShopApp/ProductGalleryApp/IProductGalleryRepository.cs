using Gtm.Contract.ProductGalleryContract.Query;
using Gtm.Domain.ShopDomain.ProductGalleryDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.ProductGalleryApp
{
    public interface IProductGalleryRepository:IRepository<ProductGallery,int>
    {
        Task<List<GalleryForProductSingleQueryModel>> GetProductSingleGalleryAsync(int productId);
    }
}
