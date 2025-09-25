using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Contract;

namespace Gtm.Contract.ProductGalleryContract.Command
{
    public class CreateProductGallery : Image_ImageAlt
    {
        public int ProductId { get; set; }
    }
}
