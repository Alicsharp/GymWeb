using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
 

namespace Gtm.Contract.EmailContract.SensEmailContract.Command
{
    public class CreateSendEmail
    {
        [Display(Name = "متن ایمیل")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Text { get; set; }
        [Display(Name = "عنوان ایمیل")]
        [MaxLength(250, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Title { get; set; }
    }
}
