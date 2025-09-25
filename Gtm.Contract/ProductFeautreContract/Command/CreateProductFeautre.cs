using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.ProductFeautreContract.Command
{
    public class CreateProductFeautre
    {
        public int ProductId { get; set; }
        [Display(Name = "عنوان")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(250, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Title { get; set; }
        [Display(Name = "مقدار")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Value { get; set; }
    }
}
