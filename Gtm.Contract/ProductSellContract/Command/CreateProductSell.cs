using System.ComponentModel.DataAnnotations;
using Utility.Appliation;

namespace Gtm.Contract.ProductSellContract.Command
{
    public class CreateProductSell
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        [Display(Name = "قیمت هر واحد فروش")]
        public int Price { get; set; }
        [Display(Name = "هر واحد فروش")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(355, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Unit { get; set; }
        [Display(Name = "وزن (برای محاسبه هزینه پست)")]
        public int Weight { get; set; }
    }
}
