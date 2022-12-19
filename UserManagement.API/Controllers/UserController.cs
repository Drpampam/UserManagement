using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagement.Core.Interfaces;
using UserManagement.Core.Services;
using UserManagement.Domain.Models;

namespace UserManagement.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        public UserController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Get details about a current logged in user
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet(Name = "get-user-profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
            var response = await _userProfileService.GetUserProfile(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get details about a user without authentication
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var response = await _userProfileService.GetUserById(id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Upload user's profile image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPatch]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await _userProfileService.UploadImageAsync(userId, file);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Delete user's profile image 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("remove-image")]
        public async Task<IActionResult> RemoveImage(string id)
        {
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = await _userProfileService.RemoveImageAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}