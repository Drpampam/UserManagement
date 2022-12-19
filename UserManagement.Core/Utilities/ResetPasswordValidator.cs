using System;
using System.Linq;
using System.Text;
using FluentValidation;
using System.Threading.Tasks;
using UserManagement.Core.DTOs;
using System.Collections.Generic;

namespace UserManagement.Core.Utilities
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDTO>
    {
        public ResetPasswordValidator()
        {
            RuleFor(ResetPasswordDTO => ResetPasswordDTO.Email).EmailAddress().WithMessage("Invalid Email").NotEmpty().NotNull();
            RuleFor(ResetPasswordDTO => ResetPasswordDTO.NewPassword).Password();
            RuleFor(ResetPasswordDTO => ResetPasswordDTO.ConfirmPassword.Equals(ResetPasswordDTO.NewPassword));
        }
    }
}
