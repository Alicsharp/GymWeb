namespace Gtm.Contract.SiteContract.MenuContract.Command
{
    public class EditMenu : UbsertMenu
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string? ImageName { get; set; }
    }
}
