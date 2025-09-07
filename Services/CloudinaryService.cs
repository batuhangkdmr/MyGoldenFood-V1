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
                    // 🚀 KALİTE ODAKLI Optimizasyon (Mobil-First)
                    Transformation = new Transformation()
                        .Width(800).Height(600).Crop("fit") // Desktop için daha büyük boyut
                        .Quality("90") // Yüksek kalite
                        .FetchFormat("auto") // WebP'yi destekleyen tarayıcılara WebP, diğerlerine JPEG
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

        // 🚀 HIZ ODAKLI Responsive resim URL'leri (Mobil-First)
        public string GetResponsiveImageUrl(string imagePath, int width, int height = 0)
        {
            if (string.IsNullOrEmpty(imagePath)) return string.Empty;
            
            // Cloudinary URL'ini manuel olarak oluştur
            var baseUrl = imagePath.Split("/upload/")[0] + "/upload/";
            var publicId = imagePath.Split("/upload/")[1];
            
            var transformation = $"w_{width}";
            if (height > 0) transformation += $",h_{height}";
            // 🎯 KALİTE ODAKLI: q_90, f_auto, fl_immutable_cache
            transformation += ",c_fit,q_90,f_auto,fl_immutable_cache";
            
            return $"{baseUrl}{transformation}/{publicId}";
        }

        // 📱 KALİTE ODAKLI Optimizasyon (Mobil trafik %90)
        public string GetMobileImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 300, 225); // Yüksek kalite mobil
        }

        public string GetTabletImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 500, 375); // Yüksek kalite tablet
        }

        public string GetDesktopImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 800, 600); // Yüksek kalite desktop
        }

        public string GetLargeImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 1200, 900);
        }

        // 🎯 Retina Display Desteği için metodlar
        public string GetUltraMobileImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 200, 150); // Ultra küçük ekranlar
        }

        public string GetSmallMobileImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 300, 225); // Küçük mobil
        }

        public string GetMediumMobileImageUrl(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 400, 300); // Orta mobil
        }

        // 🖼️ Retina Display Desteği (2x ve 3x)
        public string GetMobileImageUrl2x(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 600, 450); // Mobil 2x
        }

        public string GetTabletImageUrl2x(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 1000, 750); // Tablet 2x
        }

        public string GetDesktopImageUrl2x(string imagePath)
        {
            return GetResponsiveImageUrl(imagePath, 1600, 1200); // Desktop 2x
        }

        // 📊 Responsive Srcset HTML oluşturucu (Retina Desteği ile)
        public string GenerateResponsiveSrcset(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return string.Empty;

            var ultraMobile = GetUltraMobileImageUrl(imagePath);
            var smallMobile = GetSmallMobileImageUrl(imagePath);
            var mediumMobile = GetMediumMobileImageUrl(imagePath);
            var tablet = GetTabletImageUrl(imagePath);
            var desktop = GetDesktopImageUrl(imagePath);

            return $"{ultraMobile} 200w, {smallMobile} 300w, {mediumMobile} 400w, {tablet} 500w, {desktop} 800w";
        }

        // 🖼️ Retina Srcset HTML oluşturucu
        public string GenerateRetinaSrcset(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return string.Empty;

            var mobile = GetMobileImageUrl(imagePath);
            var mobile2x = GetMobileImageUrl2x(imagePath);
            var tablet = GetTabletImageUrl(imagePath);
            var tablet2x = GetTabletImageUrl2x(imagePath);
            var desktop = GetDesktopImageUrl(imagePath);
            var desktop2x = GetDesktopImageUrl2x(imagePath);

            return $"{mobile} 300w, {mobile2x} 600w, {tablet} 500w, {tablet2x} 1000w, {desktop} 800w, {desktop2x} 1600w";
        }

        // 🖼️ Picture element HTML oluşturucu (Retina Desteği ile)
        public string GeneratePictureElement(string imagePath, string altText = "", string cssClass = "")
        {
            if (string.IsNullOrEmpty(imagePath)) return string.Empty;

            var ultraMobile = GetUltraMobileImageUrl(imagePath);
            var smallMobile = GetSmallMobileImageUrl(imagePath);
            var mediumMobile = GetMediumMobileImageUrl(imagePath);
            var tablet = GetTabletImageUrl(imagePath);
            var desktop = GetDesktopImageUrl(imagePath);

            return $@"
                <picture>
                    <source media=""(max-width: 320px)"" srcset=""{ultraMobile}"">
                    <source media=""(max-width: 480px)"" srcset=""{smallMobile}"">
                    <source media=""(max-width: 768px)"" srcset=""{mediumMobile}"">
                    <source media=""(max-width: 1024px)"" srcset=""{tablet}"">
                    <img src=""{desktop}"" 
                         srcset=""{GenerateRetinaSrcset(imagePath)}""
                         sizes=""(max-width: 320px) 200px, (max-width: 480px) 300px, (max-width: 768px) 400px, (max-width: 1024px) 500px, 800px""
                         alt=""{altText}""
                         class=""{cssClass}""
                         loading=""lazy"">
                </picture>";
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
