using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core.Interface;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
    }
}
