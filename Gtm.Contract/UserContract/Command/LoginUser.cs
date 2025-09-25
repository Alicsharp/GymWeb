using System.ComponentModel.DataAnnotations;
using Utility.Appliation;

namespace Gtm.Contract.UserContract.Command
{
    public class LoginUser
    {
        [Display(Name = "شماره همراه")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MobileValidation(ErrorMessage = ValidationMessages.MobileErrorMessage)]
        public string Mobile { get; set; }
        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Email { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
