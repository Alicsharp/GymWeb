using System.ComponentModel.DataAnnotations;
using Utility.Contract;

namespace Gtm.Contract.ProductContract.Command
{
    public class EditProduct : Text_ShortDescription_Title_Slug_Image_ImageAlt
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
        [Display(Name = "وزن")]
        public int Weight { get; set; }
        public List<int> Categoryids { get; set; }
    }
}
