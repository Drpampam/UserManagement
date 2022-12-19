using FluentValidation;
using UserManagement.Core.DTOs;

namespace UserManagement.Core.Utilities
{
    public class UserValidator : AbstractValidator<RegistrationDTO>
    {
        public UserValidator()
        {
            RuleFor(RegistrationDTO => RegistrationDTO.Password)
                .Password();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
                .WithMessage("Passwords do not match");
            RuleFor(RegistrationDTO => RegistrationDTO.FirstName)
                .HumanName();
            RuleFor(RegistrationDTO => RegistrationDTO.LastName)
                .HumanName();
            RuleFor(RegistrationDTO => RegistrationDTO.PhoneNumber)
                .PhoneNumber();
            RuleFor(RegistrationDTO => RegistrationDTO.Email)
                .EmailAddress();
            RuleFor(RegistrationDTO => RegistrationDTO.Pin)
            .Matches(@"^[0-9]{4}$");
        }
    }
}
