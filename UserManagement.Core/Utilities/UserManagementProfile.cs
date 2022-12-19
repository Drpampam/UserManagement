using AutoMapper;
using UserManagement.Core.DTOs;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Utilities
{
    public class UserManagementProfile: Profile
    {
        public UserManagementProfile()
        {
            CreateMap<RegistrationDTO, AppUser>()
                 .ForMember(dest => dest.Email, act => act.MapFrom(src => src.Email.ToLower()))
                 .ForMember(dest => dest.UserName, act => act.MapFrom(src => src.Email.ToLower()));
            CreateMap<GetProfileDTO, AppUser>().ReverseMap();
            CreateMap<AppUser, GetUserDTO>().ReverseMap();
        }
    }
}
