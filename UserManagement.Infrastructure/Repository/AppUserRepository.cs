using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserManagement.Core.Interfaces;
using UserManagement.Domain.Models;

namespace UserManagement.Infrastructure.Repository
{
    public class AppUserRepository : GenericRepository<AppUser>, IAppUserRepository
    {
        private readonly UserManagementDbContext _context;
        private readonly DbSet<AppUser> _db;
        public AppUserRepository(UserManagementDbContext context) : base(context)
        {
            _context = context;
            _db = context.Set<AppUser>();
        }
    }
}
