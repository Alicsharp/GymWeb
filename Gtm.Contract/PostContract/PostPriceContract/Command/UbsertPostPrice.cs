using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.PostContract.PostPriceContract.Command
{
    public class UbsertPostPrice
    {
        [Display(Name = " وزن از (گرم) ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int Start { get; set; }
        [Display(Name = " وزن تا (گرم) ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int End { get; set; }
        [Display(Name = "قیمت درون شهری تهران ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int TehranPrice { get; set; }
        [Display(Name = "قیمت درون شهری مراکز استان ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int StateCenterPrice { get; set; }
        [Display(Name = "قیمت درون شهری شهرستان ها ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int CityPrice { get; set; }
        [Display(Name = "قیمت درون استانی ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int InsideStatePrice { get; set; }
        [Display(Name = "قیمت برون استانی همجوار ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int StateClosePrice { get; set; }
        [Display(Name = "قیمت برون استانی غیر همجوار ")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int StateNonClosePrice { get; set; }
    }
}
