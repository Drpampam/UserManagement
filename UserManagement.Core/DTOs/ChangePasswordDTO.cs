using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UserManagement.Core.DTOs
{
    public class ChangePasswordDTO
    {
        [Required, DataType(DataType.Password), Display(Name = "Current password")]
        public string CurrentPassword { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "New password")]
        public string NewPassword { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Confirm new password does not match")]
        public string ConfirmNewPassword { get; set; }
    }
}
