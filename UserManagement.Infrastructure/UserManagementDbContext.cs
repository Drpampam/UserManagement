using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using UserManagement.Domain.Models;

namespace UserManagement.Infrastructure
{
    public class UserManagementDbContext: IdentityDbContext<AppUser>
    {
        private const string UPDATEDAT = "UpdatedAt";
        private const string CREATEDAT = "CreatedAt";
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
        {

        }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<Address> Address { get; set; }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in ChangeTracker.Entries())
            {
                if (item.Entity is AppUser appUser)
                {
                    AuditPropertiesChange(item.State, appUser);
                }
                else if(item.Entity is Address address)
                {
                    AuditPropertiesChange(item.State, address);
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
            
        public static void AuditPropertiesChange<T>(EntityState state, T obj) where T : class
        {
            PropertyInfo? value;
            switch (state)
            {
                case EntityState.Modified:
                    value = obj.GetType().GetProperty(UPDATEDAT);
                    if(value != null)
                        value.SetValue(obj, DateTimeOffset.UtcNow);
                    break;  
                case EntityState.Added:
                    value = obj.GetType().GetProperty(CREATEDAT);
                    if(value != null) 
                        value.SetValue(obj, DateTimeOffset.UtcNow);
                    value = obj.GetType().GetProperty(UPDATEDAT);
                    if (value != null)
                        value.SetValue(obj, DateTimeOffset.UtcNow);
                    break;
                default:
                    break;
            }
        }
    }
}
