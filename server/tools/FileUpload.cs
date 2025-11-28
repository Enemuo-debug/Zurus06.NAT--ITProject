using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace server.tools
{
    public class FileUpload
    {
        private readonly Cloudinary _cloudinary;
        public FileUpload()
        {
            var acc = new Account(
                Environment.GetEnvironmentVariable("CloudName"),
                Environment.GetEnvironmentVariable("ApiKey"),
                Environment.GetEnvironmentVariable("ApiSecret")
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file.Length <= 0) return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "Zurus06.NAT-IMAGES",
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }        
    }
}