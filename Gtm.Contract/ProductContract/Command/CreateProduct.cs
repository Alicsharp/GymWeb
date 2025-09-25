using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Contract;

namespace Gtm.Contract.ProductContract.Command
{
    public class CreateProduct : Text_ShortDescription_Title_Slug_Image_ImageAlt
    {
        [Display(Name = "وزن")]
        public int Weight { get; set; }
        public List<int> Categoryids { get; set; }
    }
}
