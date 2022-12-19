using FluentValidation;

namespace UserManagement.Core.DTOs
{
    public class ForgotPasswordDTO
    {
   
        public string EmailAddress { get; set; }

    }
    public class EmailValidator : AbstractValidator<ForgotPasswordDTO>
    {
        public EmailValidator() 
        {
            RuleFor(s => s.EmailAddress).NotEmpty().EmailAddress();
        }
    }
}
