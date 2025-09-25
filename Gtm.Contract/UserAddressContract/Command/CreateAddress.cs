using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Contract.UserAddressContract.Command
{
    public class CreateAddress
    {
        public int UserId { get; set; }
        [Display(Name = "استان")]
        public int StateId { get; set; }
        [Display(Name = "شهر")]
        public int CityId { get; set; }
        [Display(Name = "جزییات آدرس")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(500, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string AddressDetail { get; set; }
        [Display(Name = "کد پستی")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(10, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [MinLength(10, ErrorMessage = ValidationMessages.MinLengthMessage)]
        public string PostalCode { get; set; }
        [Display(Name = "شماره تماس تحویل گیرنده *")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(11, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [MinLength(11, ErrorMessage = ValidationMessages.MinLengthMessage)]
        public string Phone { get; set; }
        [Display(Name = "نام تحویل گیرنده *")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string FullName { get; set; }
        [Display(Name = "کد ملی")]
        [MaxLength(10, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [MinLength(10, ErrorMessage = ValidationMessages.MinLengthMessage)]
        public string? IranCode { get; set; }
    }
}
