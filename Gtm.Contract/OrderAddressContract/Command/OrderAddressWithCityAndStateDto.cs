namespace Gtm.Contract.OrderAddressContract.Command
{
    public class OrderAddressWithCityAndStateDto
    {
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public int CityId { get; set; }
        public string FullName { get; set; }
        public string IranCode { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public int StateId { get; set; }
    }

}
