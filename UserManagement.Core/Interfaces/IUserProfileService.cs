using Microsoft.AspNetCore.Http;
using UserManagement.Core.DTOs;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface IUserProfileService
    {
        Task<ResponseDto<string>> UploadImageAsync(string id, IFormFile file);
        Task<ResponseDto<string>> RemoveImageAsync(string id);
        Task<ResponseDto<GetProfileDTO>> GetUserProfile(string Id);
        Task<ResponseDto<GetUserDTO>> GetUserById(string id);
    }
}