using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserManagement.API;
using UserManagement.API.Extensions;
using UserManagement.Core.Utilities;
using UserManagement.Infrastructure;
using UserManagement.Core.Middleware;

try
{
    // serilog setup
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var isDevelopment = environment == Environments.Development;
    IConfiguration config = ConfigurationSetUp.GetConfig(isDevelopment);
    LogSettings.SetupSerilog(config);
    Log.Logger.Information("the application has started");
    // builder class instantiation
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    // Add services to the container.
    builder.Services.AddRegisterServices(configuration);
    builder.Services.AddControllersExtension();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();
    builder.Services.AddHttpClient();
    builder.Services.AddCors();
    builder.Services.AddAuthenticationExtension(configuration);
    builder.Services.AddAuthorizationExtension();
    builder.Services.AddAutoMapper(typeof(UserManagementProfile));
    builder.Services.AddDbContextAndConfigurations(builder.Environment, builder.Configuration);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();
    builder.Services.AddCors();

    var app = builder.Build();
    await UserManagementDbInitializer.Seed(app);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(setupAction =>
        {
            setupAction.SwaggerEndpoint("/swagger/FintechOpenAPI/swagger.json", "Fintech API");
        });
    }
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex.StackTrace, "the application did not startup well");
    throw;
}
finally
{
    Log.CloseAndFlush();
}