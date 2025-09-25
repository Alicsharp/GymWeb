using System.ComponentModel.DataAnnotations;
using Utility.Appliation;

namespace Gtm.Contract.WalletContract.Command
{
    public class CreateWallet
    {
        public int UserId { get; set; }
        [Display(Name = "مبلغ (تومان)")]
        public int Price { get; set; }
        [Display(Name = "توضیحات")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public string Description { get; set; }
    }
}
