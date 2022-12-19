using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.Mime;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace UserManagement.API.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// Validates, Register and Creates Wallet for User
        /// </summary>
        /// <returns>Registration Status.</returns>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationDTO model)
        {
            var result = await _authService.Register(model);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Resent OTP for confirmation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOTP([FromBody] ResendOtpDTO model)
        {
            var response = await _authService.ResendOTP(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Verify email with token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDTO request)
        {
            var response = await _authService.ConfirmEmail(request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Authenticate a user that has access to our application
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var response = await _authService.Login(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// External login with Google Authentication
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("google")]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequestDTO request)
        {
            var response = await _authService.VerifyGoogleToken(request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Generate a reset token and send to user email address
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("forget-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var response = await _authService.ForgotPassword(model);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Reset password of a logged out user
        /// </summary>
        /// <param name="resetPasswordDTO"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDTO);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Change password of a logged in user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var userId = HttpContext.User.FindFirst(user => user.Type == ClaimTypes.NameIdentifier).Value;
            var response = await _authService.ChangePassword(model, userId);

            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Refresh token of a logged in user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO model)
        {
            var response = await _authService.RefreshTokenAsync(model);
            return StatusCode(response.StatusCode, response);
        }
    }
}