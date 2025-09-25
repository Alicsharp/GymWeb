using Utility.Domain.Enums;

namespace Gtm.Contract.SiteContract.MenuContract.Query
{
    public class MenuPageAdminQueryModel
    {
        public int Id { get; set; }
        public string PageTitle { get; set; }
        public MenuStatus? Status { get; set; }
        public List<MenuForAdminQueryModel> Menus { get; set; }
    }
}
