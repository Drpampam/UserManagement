using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Interface;

namespace UserManagement.Domain.Models
{
    public class AppUser : IdentityUser, IAuditable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }    
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public string? ImageUrl { get; set; }
        public string? Publicid { get; set; }
        public AppUser() => Addresses = new HashSet<Address>();
    }
}