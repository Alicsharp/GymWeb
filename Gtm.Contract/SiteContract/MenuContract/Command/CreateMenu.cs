using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.SiteContract.MenuContract.Command
{
    public class CreateMenu : UbsertMenu
    {
        [Display(Name = "نوع منو")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public MenuStatus Status { get; set; }
    }
}
