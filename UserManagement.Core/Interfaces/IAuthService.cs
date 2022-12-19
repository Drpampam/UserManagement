using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UserManagement.Core.DTOs;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<string>> ChangePassword(ChangePasswordDTO model, string userId);
        Task<ResponseDto<CredentialResponseDTO>> VerifyGoogleToken(GoogleLoginRequestDTO google);
        Task<ResponseDto<CredentialResponseDTO>> Login(LoginDTO model);
        Task<ResponseDto<RefreshTokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshToken);
        Task<ResponseDto<string>> ResendOTP(ResendOtpDTO model);
        Task<ResponseDto<string>> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
        Task<ResponseDto<RegistrationResponseDTO>> Register(RegistrationDTO model); 
        Task<ResponseDto<string>> ConfirmEmail(ConfirmEmailDTO confirmEmailDTO);
        Task<ResponseDto<string>> ForgotPassword(ForgotPasswordDTO model);
    }
}
