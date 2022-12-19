using System;
using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Models;

namespace UserManagement.Core.Interfaces
{
    public interface IDigitTokenService
    {
        Task<string> GenerateAsync(string purpose, UserManager<AppUser> manager, AppUser user);
        Task<bool> ValidateAsync(string purpose, string token, UserManager<AppUser> manager, AppUser user);
    }
}