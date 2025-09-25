using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.SiteContract.SliderContract.Command
{
    public class CreateSlider
    {
        [Display(Name = "تصویر")]
        public IFormFile? ImageFile { get; set; }
        [Display(Name = "Alt تصویر")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(250, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string ImageAlt { get; set; }
        [Display(Name = "لینک مقصد")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(900, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Url { get; set; }
    }
}
