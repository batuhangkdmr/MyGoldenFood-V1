using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyGoldenFood.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadImageAsync(IFormFile imageFile, string folder)
        {
            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                    Folder = folder,
                    // 🚀 WebP + Responsive + Optimizasyon
                    Transformation = new Transformation()
                        .Width(800).Height(600).Crop("fit")
                        .Quality("auto:good")
                        .FetchFormat("auto") // WebP'yi destekleyen tarayıcılara WebP, diğerlerine JPEG
                        .Flags("progressive") // Progressive JPEG
                        .Flags("immutable_cache") // Cache optimizasyonu
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                return uploadResult.Error == null ? uploadResult.SecureUrl.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                var publicId = imagePath.Replace("https://res.cloudinary.com/dbhogeepn/image/upload/", "").Split(".")[0];
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                return deletionResult.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        // 🆕 Responsive resim URL'leri için yeni metodlar
        public string GetResponsiveImageUrl(string imagePath, int width, int height = 0)
        {
            if (string.IsNullOrEmpty(imagePath)) return string.Empty;
            
            // Cloudinary URL'ini manuel olarak oluştur
            var baseUrl = imagePath.Split("/upload/")[0] + "/upload/";
            var publicId = imagePath.Split("/upload/")[1];
            
            var transformation = $"w_{width}";
            if (height > 0) transformation += $",h_{height}";
            transformation += ",c_fit,q_auto:good,f_auto,fl_progressive,fl_immutable_cache";
            
            return $"{baseUrl}{transformation}/{publicId}";
        }

        // 🎯 Farklı ekran boyutları için optimize edilmiş URL'ler
        public string GetMobileImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 400, 300);
        }

        public string GetTabletImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 600, 450);
        }

        public string GetDesktopImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 800, 600);
        }

        public string GetLargeImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 1200, 900);
        }

        // 🚀 Batch upload için toplu yükleme
        public async Task<List<string>> BatchUploadImagesAsync(List<IFormFile> imageFiles, string folder)
        {
            var tasks = imageFiles.Select(file => UploadImageAsync(file, folder));
            var results = await Task.WhenAll(tasks);
            return results.Where(url => !string.IsNullOrEmpty(url)).ToList();
        }

        // 📊 Resim bilgilerini al (boyut, format vb.)
        public async Task<ImageInfo?> GetImageInfoAsync(string imagePath)
        {
            try
            {
                var publicId = imagePath.Replace("https://res.cloudinary.com/dbhogeepn/image/upload/", "").Split(".")[0];
                var result = await _cloudinary.GetResourceAsync(publicId);
                
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new ImageInfo
                    {
                        Width = result.Width,
                        Height = result.Height,
                        Format = result.Format,
                        SizeBytes = result.Bytes,
                        PublicId = result.PublicId
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    // 📊 Resim bilgileri için model
    public class ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string PublicId { get; set; } = string.Empty;
    }
}
