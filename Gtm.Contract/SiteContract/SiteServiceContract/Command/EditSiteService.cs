namespace Gtm.Contract.SiteContract.SiteServiceContract.Command
{
    public class EditSiteService : CreateSiteService
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
    }
}
