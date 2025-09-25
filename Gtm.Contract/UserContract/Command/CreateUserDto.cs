using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Contract.UserContract.Command
{
    public class CreateUserDto
    {
        [Display(Name = "نام کامل")]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string? FullName { get; set; }

        [Display(Name = "شماره همراه")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MobileValidation(ErrorMessage = ValidationMessages.MobileErrorMessage)]
        public string Mobile { get; set; }

        [Display(Name = "ایمیل")]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [PasswordValidation(ErrorMessage = ValidationMessages.PasswordErrorMessage)]
        public string Password { get; set; }

        //[Display(Name = "تکرار کلمه عبور")]
        //[Compare(nameof(Password), ErrorMessage = "پسورد و تکرار آن مطابقت ندارند.")]
        //public string ConfirmPassword { get; set; }

        [Display(Name = "تصویر کاربری")]
        //[FileExtensions(Extensions = "jpg,png,jpeg", ErrorMessage = "فرمت تصویر باید jpg, png یا jpeg باشد.")]
        //[MaxFileSize(5 * 1024 * 1024, ErrorMessage = "حجم تصویر نباید بیشتر از ۵ مگابایت باشد.")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "جنسیت")]
        [Range(0, 2, ErrorMessage = "جنسیت نامعتبر است.")]
        public Gender UserGender { get; set; }
    }
}
