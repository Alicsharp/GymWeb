using Utility.Domain.Enums;

namespace Gtm.Contract.PostContract.CityContract.Command
{
    public class CreateCityModel
    {
        public int StateId { get; set; }
        public string Title { get; set; }
        public CityStatus Status { get; set; }
    }
}
