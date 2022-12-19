using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Core.DTOs
{
    public class CreateWalletResponseDTO
    {
        public string Id { get; set; }
        public string PaystackCustomerCode { get; set; }       
    }
}
