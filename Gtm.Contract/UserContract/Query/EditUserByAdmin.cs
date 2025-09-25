using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.UserContract.Query
{
    public class EditUserByAdmin
    {
        public int Id { get; set; }
        [Display(Name = "نام کامل")]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string? FullName { get; set; }
        [Display(Name = "شماره همراه")]
        [MobileValidation(ErrorMessage = ValidationMessages.MobileErrorMessage)]
        public string Mobile { get; set; }
        [Display(Name = "ایمیل")]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string? Email { get; set; }
        [Display(Name = "کلمه عبور")]
        [MaxLength(8, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [MinLength(5, ErrorMessage = ValidationMessages.MinLengthMessage)]
        public string? Password { get; set; }
        public string AvatarName { get; set; }
        [Display(Name = "تصویرکاربری")]
        public IFormFile? AvatarFile { get; set; }
        [Display(Name = "جنسیت")]
        public Gender UserGender { get; set; }
    }
}
