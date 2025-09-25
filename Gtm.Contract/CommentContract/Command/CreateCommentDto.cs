using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.CommentContract.Command
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "شناسه کاربر ارسال‌کننده الزامی است.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "شناسه مالک (مقاله/محصول/صفحه) الزامی است.")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "نوع کامنت الزامی است.")]
        public CommentFor CommentFor { get; set; }

        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [MaxLength(100, ErrorMessage = "ایمیل نمی‌تواند بیش از ۱۰۰ کاراکتر باشد.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "متن کامنت الزامی است.")]
        [MinLength(5, ErrorMessage = "متن کامنت باید حداقل ۵ کاراکتر باشد.")]
        [MaxLength(1000, ErrorMessage = "متن کامنت نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد.")]
        public string Text { get; set; }

        public long? ParentId { get; set; } // برای کامنت‌های پاسخ‌دهی
    }
}
