using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;
using ILogger = Serilog.ILogger;

namespace UserManagement.Infrastructure.ExternalServices
{
    public class CloudinaryServices : ICloudinaryServices
    {
        private ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;

        public CloudinaryServices(IServiceProvider provider, IConfiguration configuration,ILogger logger)
        {
            _logger = logger;
            _configuration = configuration;
            _cloudinary = new Cloudinary(new Account(_configuration.GetValue<string>("CloudinarySettings:CloudName"),
                                                     _configuration.GetValue<string>("CloudinarySettings:ApiKey"), 
                                                     _configuration.GetValue<string>("CloudinarySettings:ApiSecret")));
        }

        private bool ValidateImage(IFormFile image)
        {
            // validate the image size and extension type using settings from appsettings
            var status = false;
            string[] listOfextensions = { ".jpg", ".jpeg", ".png" };
            if(image == null) return status;
            for (int i = 0; i < listOfextensions.Length; i++)
            {
                if (image.FileName.EndsWith(listOfextensions[i]))
                {
                    status = true;
                    break;
                }
            }
            if (status)
            {
                var pixSize = Convert.ToInt64(_configuration.GetValue<string>("PhotoSettings:Size"));
                if (image.Length > pixSize)
                    return !status;
            }
            return status;
        }

        /// <summary>
        /// Uploads a single image
        /// </summary>
        /// <param name="file">Image data</param>
        /// <returns></returns>
        public async Task<UploadResult> UploadImage(IFormFile file)
        {
            _logger.Information("Enter the upload image service");
            //Runtime Complexity Check needed.
            var result = ValidateImage(file);
            _logger.Information($"Check result {result}");
            var uploadResult = new ImageUploadResult();
            if (!result)
            {
                return default;
            }
            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            using (var imageStream = file.OpenReadStream())
            {
                _logger.Information($"Check the imageStream {imageStream}");
                var parameters = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, imageStream),
                    PublicId = fileName
                };
                _logger.Information(" Finish Image upload Setup, about load to cloundinary ");
                uploadResult = await _cloudinary.UploadAsync(parameters);
            };
            _logger.Information(" Upload done to cloudinary successfully");
            return uploadResult;
        }

        public async Task<UploadResult> UpdateByPublicId(IFormFile file, string publicId)
        {
            var response = new ResponseDto<bool>();
            var uploadResult = new ImageUploadResult();
            await using var imageStream = file.OpenReadStream();
            var parameters = new ImageUploadParams()
            {
                File = new FileDescription(publicId, imageStream),
                PublicId = publicId,
                Overwrite = true,
                UniqueFilename = true
            };
            _logger.Information(" Finish Image upload Setup, about load to cloundinary ");
            uploadResult = await _cloudinary.UploadAsync(parameters);
            return uploadResult;
        }

        public async Task<bool> DeleteByPublicId(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deleteParams);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}