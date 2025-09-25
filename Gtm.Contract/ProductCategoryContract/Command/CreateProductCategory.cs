using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Contract;

namespace Gtm.Contract.ProductCategoryContract.Command
{
    public class CreateProductCategory : Title_Slug_Image_ImageAlt
    {
        public int Parent { get; set; }
    }
}
