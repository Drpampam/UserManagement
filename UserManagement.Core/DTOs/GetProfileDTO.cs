namespace UserManagement.Core.DTOs
{
    public class GetProfileDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Publicid { get; set; }
        public AddressDTO Address { get; set; }
    }
}
