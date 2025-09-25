using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.TagContract.Command
{
    public class CreateTagDto
    {
        [Required(ErrorMessage = "عنوان تگ الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان تگ نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string MetaDescription { get; set; }
    }
}
