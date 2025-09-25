using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.SiteContract.ImageSiteContract.Command
{
    public class CreateImageSite
    {
        [Display(Name = "تصویر")]
        [Required(ErrorMessage = ValidationMessages.ImageErrorMessage)]
        public IFormFile ImageFile { get; set; }
        [Display(Name = "عنوان")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(250, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Title { get; set; }
    }
}
