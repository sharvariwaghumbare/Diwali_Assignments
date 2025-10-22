namespace ECommerce.Application.DTOs.Address
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FullAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}
