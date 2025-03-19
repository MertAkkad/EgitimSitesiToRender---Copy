using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
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
        
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = "gallery")
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
        
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return null;
                
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        
        public string GetImageUrl(string publicId, int width = 800, int height = 600)
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
    }
} 