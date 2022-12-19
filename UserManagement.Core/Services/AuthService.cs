using System.Net;
using Google.Apis.Auth;
using UserManagement.Core.DTOs;
using ILogger = Serilog.ILogger;
using UserManagement.Domain.Models;
using Microsoft.AspNetCore.Identity;
using UserManagement.Core.Utilities;
using UserManagement.Core.Interfaces;
using UserManagement.Core.AppSettings;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using UserManagement.Domain.Enums;
using Microsoft.Extensions.Configuration;
using static Google.Apis.Requests.BatchRequest;

namespace UserManagement.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IDigitTokenService _digitTokenService;
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly GoogleSettings _googleSettings;
        private readonly NotificationSettings _notificationSettings;
        private readonly PaymentSettings _paymentSettings;
        public AuthService(IServiceProvider provider)
        {
            _userManager = provider.GetRequiredService<UserManager<AppUser>>();
            _tokenService = provider.GetRequiredService<ITokenService>();
            _digitTokenService = provider.GetRequiredService<IDigitTokenService>();
            _httpClientService = provider.GetRequiredService<IHttpClientService>();
            _mapper = provider.GetRequiredService<IMapper>();
            _logger = provider.GetRequiredService<ILogger>();
            _googleSettings = provider.GetRequiredService<GoogleSettings>();
            _notificationSettings = provider.GetRequiredService<NotificationSettings>();
            _paymentSettings = provider.GetRequiredService<PaymentSettings>();
        }

        public async Task<ResponseDto<string>> ForgotPassword(ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.EmailAddress);
            if (user is null)
                return ResponseDto<string>.Fail("This email does not exist on this app", (int)HttpStatusCode.NotFound);
            var purpose = UserManager<AppUser>.ResetPasswordTokenPurpose;
            var token = await _digitTokenService.GenerateAsync(purpose, _userManager, user);
            var mailBody = await EmailBodyBuilder.GetEmailBody(user, "StaticFiles/ForgotPassword.html", token);
            var emailNotification = new EmailNotificationDTO
            {
                ToEmail = user.Email,
                Subject = "Reset Password",
                Message = mailBody,
            };

            try
            {
                await _httpClientService.PostRequestAsync<EmailNotificationDTO,
                    ResponseDto<bool>>(_notificationSettings.BaseUrl, "api/v1/Notification/send-email", emailNotification);

                return ResponseDto<string>.Success($"This email is successfully to: {model.EmailAddress}",
                        $"A reset link was successfully sent to {model.EmailAddress}", (int)HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return ResponseDto<string>.Fail("Service is not available, please try again later.", (int)HttpStatusCode.ServiceUnavailable);
            }
        }

        public async Task<ResponseDto<CredentialResponseDTO>> Login(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ResponseDto<CredentialResponseDTO>.Fail("User does not exist", (int)HttpStatusCode.NotFound);
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ResponseDto<CredentialResponseDTO>.Fail("Invalid user credential", (int)HttpStatusCode.BadRequest);
            }

            if (!user.EmailConfirmed)
            {
                return ResponseDto<CredentialResponseDTO>.Fail("User's account is not confirmed", (int)HttpStatusCode.BadRequest);
            }
            else if (!user.IsActive)
            {
                return ResponseDto<CredentialResponseDTO>.Fail("User's account is deactivated", (int)HttpStatusCode.BadRequest);
            }

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); //sets refresh token for 7 days
            var credentialResponse = new CredentialResponseDTO()
            {
                Id = user.Id,
                Token = await _tokenService.GenerateToken(user),
                RefreshToken = user.RefreshToken
            };

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.Information("User successfully logged in");
                return ResponseDto<CredentialResponseDTO>.Success("Login successful", credentialResponse);
            }
            return ResponseDto<CredentialResponseDTO>.Fail("Failed to login user", (int)HttpStatusCode.InternalServerError);
        }

        public async Task<ResponseDto<RefreshTokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshToken)
        {
            var response = new ResponseDto<RefreshTokenResponseDTO>();
            var tokenToBeRefreshed = refreshToken.RefreshToken;
            var userId = refreshToken.UserId;

            var user = await _userManager.FindByIdAsync(userId);
            int value = DateTime.Compare((DateTime)user?.RefreshTokenExpiryTime!, DateTime.Now);
            if (user.RefreshToken != tokenToBeRefreshed || value < 0)
            {
                return ResponseDto<RefreshTokenResponseDTO>.Fail("Invalid credentials", (int)HttpStatusCode.BadRequest);
            }
            var refreshMapping = new RefreshTokenResponseDTO
            {
                NewAccessToken = await _tokenService.GenerateToken(user),
                NewRefreshToken = _tokenService.GenerateRefreshToken()
            };

            user.RefreshToken = refreshMapping.NewRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return ResponseDto<RefreshTokenResponseDTO>.Success("Token refreshed successfully", refreshMapping);
        }

        public async Task<ResponseDto<string>> ResendOTP(ResendOtpDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ResponseDto<string>.Fail("Email does not exist", (int)HttpStatusCode.NotFound);
            }

            var purpose = (model.Purpose == "ConfirmEmail") ? UserManager<AppUser>.ConfirmEmailTokenPurpose
                : UserManager<AppUser>.ResetPasswordTokenPurpose;
            var token = await _digitTokenService.GenerateAsync(purpose, _userManager, user);

            var mailBody = await EmailBodyBuilder.GetEmailBody(user, emailTempPath: (model.Purpose == "ConfirmEmail") ?
                "StaticFiles/EmailConfirmation.html" : "StaticFiles/ForgotPassword.html", token);

            var emailNotification = new EmailNotificationDTO
            {
                ToEmail = user.Email,
                Subject = "Email Verification",
                Message = mailBody,
            };

            var notificationService = await _httpClientService.PostRequestAsync<EmailNotificationDTO,
                ResponseDto<bool>>(_notificationSettings.BaseUrl, "send-email", emailNotification);

            if (notificationService.Data)
                if (!user.IsActive)
                {
                    return ResponseDto<string>.Success($"This email is successfully to: {model.Email}",
                        $"OTP was successfully resent to {model.Email}");
                }
            return ResponseDto<string>.Fail("Sending OTP was not successful", (int)HttpStatusCode.InternalServerError);
        }

        public async Task<ResponseDto<string>> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {

            var validator = new ResetPasswordValidator();
            await validator.ValidateAsync(resetPasswordDTO);
            _logger.Information("Reset password attempt");
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                return ResponseDto<string>.Fail("Email does not exist", (int)HttpStatusCode.NotFound);
            }
            var purpose = UserManager<AppUser>.ResetPasswordTokenPurpose;
            var isValidToken = await _digitTokenService.ValidateAsync(purpose, resetPasswordDTO.Token, _userManager, user);
            var result = new IdentityResult();
            var hasher = new PasswordHasher<AppUser>();
            if (isValidToken)
            {
                var hash = hasher.HashPassword(user, resetPasswordDTO.NewPassword);
                user.PasswordHash = hash;
                result = await _userManager.UpdateAsync(user);
            }
            if (result.Succeeded)
            {
                return ResponseDto<string>.Success("Password has been reset successfully", user.Id, (int)HttpStatusCode.OK);
            }
            return ResponseDto<string>.Fail("Invalid Token", (int)HttpStatusCode.BadRequest);
        }

        public async Task<ResponseDto<string>> ConfirmEmail(ConfirmEmailDTO confirmEmailDTO)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDTO.EmailAddress);
            if (user == null)
            {
                return ResponseDto<string>.Fail("User not found", (int)HttpStatusCode.NotFound);
            }
            var purpose = UserManager<AppUser>.ConfirmEmailTokenPurpose;
            var result = await _digitTokenService.ValidateAsync(purpose, confirmEmailDTO.Token, _userManager, user);
            if (result)
            {
                user.EmailConfirmed = true;
                user.IsActive = true;//favour said i should do this so its her fault
                var update = await _userManager.UpdateAsync(user);
                if (update.Succeeded)
                {
                    return ResponseDto<string>.Success("Email Confirmation successful", user.Id, (int)HttpStatusCode.OK);
                }
            }
            return ResponseDto<string>.Fail("Email Confirmation not successful", (int)HttpStatusCode.Unauthorized);
        }

        public async Task<ResponseDto<RegistrationResponseDTO>> Register(RegistrationDTO userDetails)
        {
            var checkEmail = await _userManager.FindByEmailAsync(userDetails.Email);
            if (checkEmail != null)
            {
                return ResponseDto<RegistrationResponseDTO>.Fail("Email already Exists", (int)HttpStatusCode.BadRequest);
            }
            var userModel = _mapper.Map<AppUser>(userDetails);
            await _userManager.CreateAsync(userModel, userDetails.Password);
            await _userManager.AddToRoleAsync(userModel, UserRole.Customer.ToString());

            var IsWalletCreated = await CreateWallet(userModel, userDetails.Pin);
            if (IsWalletCreated == null || !IsWalletCreated.Status)
                return ResponseDto<RegistrationResponseDTO>.Fail("Internal Server Error", IsWalletCreated.StatusCode);

            var sendEmailResponse = await SendEmail(userModel);
            if (sendEmailResponse == null || !sendEmailResponse.Status)
                return ResponseDto<RegistrationResponseDTO>.Fail("Registration successful, but resent otp for email verification", 
                    sendEmailResponse.StatusCode);

            return ResponseDto<RegistrationResponseDTO>.Success("Registration Successful",
                new RegistrationResponseDTO{Id= userModel.Id,Email=userModel.Email },
                (int)HttpStatusCode.Created);
        }

        private async Task<ResponseDto<CreateWalletResponseDTO>> CreateWallet(AppUser userModel, string pin)
        {
            var walletModel = new CreateWalletDTO
            {
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                UserEmail = userModel.Email,
                Pin = pin,
                UserId = userModel.Id
            };
            try
            {
                return await _httpClientService.PostRequestAsync<CreateWalletDTO, ResponseDto<CreateWalletResponseDTO>>
                (_paymentSettings.BaseUrl, "api/Wallets/create-wallet", walletModel);
            }
            catch (Exception e)
            {
                return ResponseDto<CreateWalletResponseDTO>.Fail(e.Message, (int) HttpStatusCode.InternalServerError);
            }
        }

        private async Task<ResponseDto<bool>> SendEmail(AppUser userModel)
        {
            var purpose = UserManager<AppUser>.ConfirmEmailTokenPurpose;
            string token = await _digitTokenService.GenerateAsync(purpose, _userManager, userModel);
            var mailBody = await EmailBodyBuilder.GetEmailBody(userModel, "StaticFiles/EmailConfirmation.html", token);
            var sendEmail = new EmailNotificationDTO
            {
                ToEmail = userModel.Email,
                Subject = "Email Verification",
                Message = mailBody
            };

            try
            {
                return await _httpClientService.PostRequestAsync<EmailNotificationDTO, ResponseDto<bool>>
                    (_notificationSettings.BaseUrl, "api/v1/Notification/send-email", sendEmail);
            }
            catch (Exception)
            {
                return ResponseDto<bool>.Fail("Service is not available, please try again later.", 
                    (int)HttpStatusCode.ServiceUnavailable);
            }
        }

        public async Task<ResponseDto<string>> ChangePassword(ChangePasswordDTO model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ResponseDto<string>.Fail("User not found.");
            }

            var isPasswordConfirmed = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!isPasswordConfirmed)
            {
                return ResponseDto<string>.Fail("Current password is incorrect.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return ResponseDto<string>.Success("Successful!", "Password has been updated");
            }

            return IdentityResultErrors<string>(result);
        }

        private static string GetErrors(IdentityResult result)
        {
            return result.Errors.Aggregate(string.Empty, (curr, err) => curr + err.Description + "\n");
        }
        
        public async Task<ResponseDto<CredentialResponseDTO>> VerifyGoogleToken(GoogleLoginRequestDTO google)
        {
            var names = google.Name.Split(' ');
            string firstName = names[0];
            string lastName = names[1];
            var Role = UserRole.Customer.ToString();
            var user = await _userManager.FindByEmailAsync(google.Email);
            if (user == null)
            {
                user = new AppUser { Email = google.Email, FirstName = firstName, LastName = lastName, UserName = google.Email };
                var response = await _userManager.CreateAsync(user);
                if(!response.Succeeded)
                {
                    _logger.Error("Could not create external login user");
                    return IdentityResultErrors<CredentialResponseDTO>(response);
                }
                response = await _userManager.AddToRoleAsync(user, Role);

                if (!response.Succeeded)
                {
                    _logger.Error("Could not create external login user");
                    return IdentityResultErrors<CredentialResponseDTO>(response);
                }
            }
            
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.EmailConfirmed = true;
            user.IsActive = true;

            var credentialResponse = new CredentialResponseDTO()
            {
                Id = user.Id,
                Token = await _tokenService.GenerateToken(user),
                RefreshToken = user.RefreshToken
            };

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.Information("User successfully logged in");
                return ResponseDto<CredentialResponseDTO>.Success("Login successful", credentialResponse);
            }
            return IdentityResultErrors<CredentialResponseDTO>(result);
        }

        private ResponseDto<T> IdentityResultErrors<T>(IdentityResult result)
        {
            return ResponseDto<T>.Fail(GetErrors(result), (int)HttpStatusCode.InternalServerError);
        }
    }
}