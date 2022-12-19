using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core.Interface;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface IAppUserRepository : IGenericRepository<AppUser>
    {
    }    
}
