namespace Gtm.Contract.PostContract.UserPostContract.Command
{
    public class EditPackage : CreatePackage
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
    }
}
