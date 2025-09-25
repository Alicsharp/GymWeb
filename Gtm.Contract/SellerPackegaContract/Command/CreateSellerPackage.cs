using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.SellerPackegaContract.Command
{
    public class CreateSellerPackage
    {
        [Display(Name = "عنوان")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(355, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Title { get; set; }
        [Display(Name = "توضیحات")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Description { get; set; }
        [Display(Name = "قیمت")]
        public int Price { get; set; }
        [Display(Name = "درصد تخفیف")]
        public int Percent { get; set; }
        [Display(Name = "ماه")]
        public int Mounth { get; set; }
    }
}
