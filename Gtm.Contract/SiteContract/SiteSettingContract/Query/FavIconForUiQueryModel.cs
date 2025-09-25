namespace Gtm.Contract.SiteContract.SiteSettingContract.Query
{
    public class FavIconForUiQueryModel
    {
        public FavIconForUiQueryModel(string? favIcon)
        {
            FavIcon = favIcon;
        }

        public string? FavIcon { get; private set; }
    }
}
