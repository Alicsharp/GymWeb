namespace Gtm.Contract.SiteContract.SliderContract.Command
{
    public class EditSlider : CreateSlider
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
    }
}
