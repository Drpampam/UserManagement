using System;
using System.Globalization;
using UserManagement.Domain.Models;
// using Serilog;

namespace UserManagement.Core.Utilities
{
	public class EmailBodyBuilder
	{
		public EmailBodyBuilder()
		{
		}

        public static async Task<string> GetEmailBody(AppUser user, string emailTempPath, string token)
        {
            TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;
            var userFirstName = textInfo.ToTitleCase(user.FirstName ?? "");
            // when use of serilog is configured
            //Log.Information("About to get the static email file");
            var temp = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), emailTempPath));
            //Log.Information($"Successfull get email path: {temp}");
            return temp.Replace("**code**", token).Replace("**user**", userFirstName);
        }
    }
}