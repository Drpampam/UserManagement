using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Core.Interfaces
{
    public interface ICloudinaryServices
    {
        Task<UploadResult> UpdateByPublicId(IFormFile file, string publicId);
        Task<UploadResult> UploadImage(IFormFile file);
        Task<bool> DeleteByPublicId (string publicId);
    }
}