using UserManagement.Core.Interfaces;
using UserManagement.Domain.Models;

namespace UserManagement.Infrastructure.Repository
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(UserManagementDbContext context) : base(context)
        {
        }
    }
}
