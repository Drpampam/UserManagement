using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Core.DTOs
{
    public class CreateWalletDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserEmail { get; set; }        
        public string UserId { get; set; }
        public string Pin { get; set; }
    }
}
