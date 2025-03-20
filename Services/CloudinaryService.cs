using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EgitimSitesi.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        
        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }
        
        public async Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string folder = "general")
        {
            if (file == null || file.Length == 0)
                return null;
                
            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation()
                    .Width(1000)
                    .Height(800)
                    .Crop("limit")
                    .Quality("auto:good")
            };
            
            return await _cloudinary.UploadAsync(uploadParams);
        }
        
        public async Task<DeletionResult?> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return null;
                
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        
        public string? GetImageUrl(string publicId, int width = 800, int height = 600)
        {
            if (string.IsNullOrEmpty(publicId))
                return null;
                
            return _cloudinary.Api.UrlImgUp.Transform(new Transformation()
                .Width(width)
                .Height(height)
                .Crop("fill")
                .Quality("auto:good"))
                .BuildUrl(publicId);
        }

        // Method to extract public ID from a Cloudinary URL
        public string? GetPublicIdFromUrl(string cloudinaryUrl)
        {
            if (string.IsNullOrEmpty(cloudinaryUrl))
                return null;
                
            // Cloudinary URLs typically look like: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{public_id}.{format}
            try
            {
                var uri = new Uri(cloudinaryUrl);
                var path = uri.AbsolutePath;
                
                // Remove /image/upload/vXXXXXXXX/ prefix
                var segments = path.Split(new[] { "/image/upload/" }, StringSplitOptions.None);
                if (segments.Length < 2)
                    return null;
                    
                // Remove extension and version number
                var publicIdWithVersion = segments[1];
                var versionSegments = publicIdWithVersion.Split('/');
                var fileName = versionSegments[versionSegments.Length - 1];
                
                // Remove file extension
                if (fileName.Contains("."))
                    fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
                    
                var path1 = string.Join("/", versionSegments.AsSpan(1, versionSegments.Length - 2));
                var path2 = string.IsNullOrEmpty(path1) ? fileName : $"{path1}/{fileName}";
                
                return path2;
            }
            catch
            {
                return null;
            }
        }
    }
} 