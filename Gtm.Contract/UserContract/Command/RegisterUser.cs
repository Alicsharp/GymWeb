using System.ComponentModel.DataAnnotations;
using Utility.Appliation;

namespace Gtm.Contract.UserContract.Command
{
    public class RegisterUser
    {
        [Display(Name = "شماره همراه")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MobileValidation(ErrorMessage = ValidationMessages.MobileErrorMessage)]
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
    }
}
