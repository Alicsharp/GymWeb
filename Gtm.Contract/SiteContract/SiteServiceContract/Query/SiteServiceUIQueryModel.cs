namespace Gtm.Contract.SiteContract.SiteServiceContract.Query
{
    public class SiteServiceUIQueryModel
    {
        public SiteServiceUIQueryModel(int id, string title, string imageName, string imageAlt)
        {
            Id = id;
            Title = title;
            ImageName = imageName;
            ImageAlt = imageAlt;
        }

        public int Id { get; private set; }
        public string Title { get; private set; }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
    }
}
