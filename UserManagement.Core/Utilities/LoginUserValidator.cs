using FluentValidation;
using UserManagement.Core.DTOs;

namespace UserManagement.Core.Utilities
{
    public class LoginUserValidator: AbstractValidator<LoginDTO>
    {
        public LoginUserValidator()
        {
            RuleFor(LoginDTO => LoginDTO.Email).NotEmpty().WithMessage("Email must not be empty").EmailAddress();
            RuleFor(LoginDTO => LoginDTO.Password).NotEmpty().WithMessage("password must not be empty").Password();    
        }
    }
}
