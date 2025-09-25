using System.ComponentModel.DataAnnotations;

namespace Gtm.Contract.TagContract.Command
{
    public class UpdateTagDto
    {
        [Required(ErrorMessage = "شناسه تگ الزامی است")]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان تگ الزامی است")]
        [MaxLength(100, ErrorMessage = "عنوان تگ نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "توضیحات متا نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string MetaDescription { get; set; }
    }
}
