using System.ComponentModel.DataAnnotations.Schema;
using UserManagement.Domain.Interface;

namespace UserManagement.Domain.Models
{
    public class Address : IAuditable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
