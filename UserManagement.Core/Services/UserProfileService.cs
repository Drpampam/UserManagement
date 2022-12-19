using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Net;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICloudinaryServices _cloudinaryServices;

        public UserProfileService(UserManager<AppUser> userManager, IMapper mapper, ICloudinaryServices cloudinaryServices)
        {
            _userManager = userManager;
            _mapper = mapper;
            _cloudinaryServices = cloudinaryServices;
        }
        public async Task<ResponseDto<GetProfileDTO>> GetUserProfile(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
                return ResponseDto<GetProfileDTO>.Fail("Invalid credentials", (int)HttpStatusCode.BadRequest);

            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
                return ResponseDto<GetProfileDTO>.Fail("User does not exist");

            var userDetails = _mapper.Map<GetProfileDTO>(user);
            return ResponseDto<GetProfileDTO>.Success("", userDetails);
        }

        public async Task<ResponseDto<string>> UploadImageAsync(string id, IFormFile file)
        {
            Log.Information("Successfull enter the image upload service");
            var user = await _userManager.FindByIdAsync(id);
            UploadResult upload;
            if (user == null)
            {
                Log.Information("User is not found");
                return ResponseDto<string>.Fail("User is not found", (int)HttpStatusCode.NotFound);
            }
            Log.Information("User is found");
            if (user.ImageUrl is not null)
            {
                upload = await _cloudinaryServices.UpdateByPublicId(file, user.Publicid);
            }
            else
            {
                upload = await _cloudinaryServices.UploadImage(file);
            }
            if (upload is null)
            {
                Log.Information("image not uploaded");
                return ResponseDto<string>.Fail("Image is not valid", (int)HttpStatusCode.ServiceUnavailable);
            }
            Log.Information("Successful upload the image");
            user.ImageUrl = upload.Url.ToString();
            user.Publicid = upload.PublicId;
            var response = await _userManager.UpdateAsync(user);
            if (!response.Succeeded)
            {
                Log.Information("imageurl not added to the database");
                await _cloudinaryServices.DeleteByPublicId(upload.PublicId);
                return ResponseDto<string>.Fail("Image was added to cloudinary but not persisted to the database", (int)HttpStatusCode.InternalServerError);
            }
            return ResponseDto<string>.Success("Image uploaded successfully", user.ImageUrl, (int)HttpStatusCode.OK);
        }

        public async Task<ResponseDto<string>> RemoveImageAsync(string id)
        {
            Log.Information("Successfull enter the remove image service");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                Log.Information("User is not found");
                return ResponseDto<string>.Fail("User is not found", (int)HttpStatusCode.NotFound);
            }
            Log.Information("User is found");
            var imageId = user.Publicid;
            if(imageId == null)
            {
                Log.Information("Image not found");
                return ResponseDto<string>.Fail("Image not found", (int)HttpStatusCode.NotFound);
            }
            await _cloudinaryServices.DeleteByPublicId(imageId);
            Log.Information("Successfully Deleted Image");
            user.ImageUrl = null;
            var response = await _userManager.UpdateAsync(user);
            return ResponseDto<string>.Success("Image deleted successfully", user.ImageUrl, (int)HttpStatusCode.OK);

        }

        public async Task<ResponseDto<GetUserDTO>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ResponseDto<GetUserDTO>.Fail("Invalid credentials", (int)HttpStatusCode.BadRequest);

            var user = await _userManager.FindByIdAsync(id);
            var returnedUser = _mapper.Map<GetUserDTO>(user);
            if (user == null)
            {
                return ResponseDto<GetUserDTO>.Fail("User Not Found");
            }
            return ResponseDto<GetUserDTO>.Success("User Found", returnedUser);
        }
    }
}
