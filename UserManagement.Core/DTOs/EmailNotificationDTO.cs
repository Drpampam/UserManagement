using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Core.DTOs
{
    public class EmailNotificationDTO
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}