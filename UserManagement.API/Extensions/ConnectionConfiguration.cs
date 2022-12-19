using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Core.Services;
using UserManagement.Core.Utilities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Models;
using UserManagement.Infrastructure;

namespace UserManagement.API.Extensions
{
    public static class ConnectionConfiguration
    {
        private static string GetHerokuConnectionString()
        {
            // Get the Database URL from the ENV variables in Heroku
            string connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL")!;
            // parse the connection string
            var databaseUri = new Uri(connectionUrl);
            string db = databaseUri.LocalPath.TrimStart('/');
            string[] userInfo = databaseUri.UserInfo.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            return $"User ID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};" +
            $"Database={db};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;";
        }

        public static void AddDbContextAndConfigurations(this IServiceCollection services, IWebHostEnvironment env, IConfiguration config)
        {
            services.AddDbContextPool<UserManagementDbContext>(options =>
            {
                string connStr;
                if (env.IsProduction())
                {
                    connStr = GetHerokuConnectionString();
                }
                else
                {
                    connStr = config.GetConnectionString("DefaultConnection");
                }
                options.UseNpgsql(connStr);
            });

            var builder = services.AddIdentity<AppUser, IdentityRole>(x =>
            {
                x.Password.RequiredLength = 8;
                x.Password.RequireDigit = false;
                x.Password.RequireUppercase = true;
                x.Password.RequireLowercase = true;
                x.User.RequireUniqueEmail = true;
                x.SignIn.RequireConfirmedEmail = true;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
            _ = builder.AddEntityFrameworkStores<UserManagementDbContext>()
            .AddTokenProvider<DigitTokenService>(DigitTokenService.DIGITEMAIL)
            .AddDefaultTokenProviders();
        }
    }
}
