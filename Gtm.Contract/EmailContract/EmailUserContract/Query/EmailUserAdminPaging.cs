using Utility.Appliation;

namespace Gtm.Contract.EmailContract.EmailUserContract.Query
{
    public class EmailUserAdminPaging : BasePaging
    {
        public List<EmailUserAdminQueryModel> Emails { get; set; }
        public string Filter { get; set; }
    }
}
