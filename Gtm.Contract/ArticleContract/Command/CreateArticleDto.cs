using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Contract;
namespace Gtm.Contract.ArticleContract.Command
{
    public class CreateArticleDto : Text_ShortDescription_Title_Slug_Image_ImageAlt
    {
        [Display(Name = "سر گروه")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int CategoryId { get; set; }
        [Display(Name = "زیرگروه")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public int SubCategoryId { get; set; }
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        public long UserId { get; set; }
        [Display(Name = "نویسنده")]
        [Required(ErrorMessage = ValidationMessages.RequiredMessage)]
        [MaxLength(255, ErrorMessage = ValidationMessages.MaxLengthMessage)]
        public string Writer { get; set; }
    }
}
