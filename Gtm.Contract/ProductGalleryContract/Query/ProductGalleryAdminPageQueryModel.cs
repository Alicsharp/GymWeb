using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ProductGalleryContract.Query
{
    public class ProductGalleryAdminPageQueryModel
    {

        public int ProductId { get; set; }
        public string Title { get; set; }
        public List<ProductGalleryAdminQueryModel> Galleries { get; set; }
    }
}
