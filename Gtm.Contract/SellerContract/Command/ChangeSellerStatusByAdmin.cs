using System.ComponentModel.DataAnnotations;

namespace Gtm.Contract.SellerContract.Command
{
    public class ChangeSellerStatusByAdmin
    {
        public int Id { get; set; }
        [Display(Name = "متن اس ام اس")]
        [Required(ErrorMessage = "متن اس ام اس الزامی است .")]
        public string DescriptionSMS { get; set; }
        [Display(Name = "متن ایمیل")]
        public string? DescriptionEmail { get; set; }
    }
}
