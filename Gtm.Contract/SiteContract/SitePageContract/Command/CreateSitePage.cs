using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Contract;

namespace Gtm.Contract.SiteContract.SitePageContract.Command
{
    public class CreateSitePage : Title_Slug
    {
        [Display(Name = "توضیح")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Text { get; set; }
    }
}
