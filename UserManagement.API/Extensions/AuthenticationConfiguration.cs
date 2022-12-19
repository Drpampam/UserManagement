using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserManagement.API.Extensions
{
    public static class AuthenticationConfiguration
    {
        public static void AddAuthenticationExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("Jwt");
            services.AddAuthentication(v =>
            {
                v.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                v.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(v =>
             {
                 v.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateLifetime = true,
                     ValidateAudience = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = jwtConfig.GetSection("Issuer").Value,
                     ValidAudience = jwtConfig.GetSection("Audience").Value,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Token")))
                 };
             });
        }
    }
}