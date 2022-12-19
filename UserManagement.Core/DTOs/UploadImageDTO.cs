using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Core.DTOs
{
    public class UploadImageDTO
    {
        public IFormFile ImageToUpload { get; set; }
    }
}
