using System.ComponentModel.DataAnnotations;

namespace Gtm.Contract.TagContract.Command
{
    public class DeleteTagDto
    {
        [Required(ErrorMessage = "شناسه تگ الزامی است")]
        public int Id { get; set; }

        public bool ForceDelete { get; set; } = false;
        public string Reason { get; set; }
    }
}
