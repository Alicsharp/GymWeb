using Utility.Domain.Enums;

namespace Gtm.Contract.PostContract.CityContract.Command
{
    public class EditCityModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public CityStatus Status { get; set; }
    }
}
