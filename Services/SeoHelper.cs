using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace MyGoldenFood.Services
{
    public static class SeoHelper
    {
        /// <summary>
        /// Canonical URL oluşturur
        /// </summary>
        public static string GetCanonicalUrl(IUrlHelper urlHelper, string path = null)
        {
            var request = urlHelper.ActionContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            
            if (string.IsNullOrEmpty(path))
            {
                path = request.Path.Value;
            }
            
            // Dil prefix'ini kaldır (örn: /tr/urunler -> /urunler)
            if (path.StartsWith("/tr/") || path.StartsWith("/en/") || 
                path.StartsWith("/de/") || path.StartsWith("/fr/") || 
                path.StartsWith("/es/") || path.StartsWith("/ru/") || 
                path.StartsWith("/ar/") || path.StartsWith("/ja/") || 
                path.StartsWith("/ko/"))
            {
                path = path.Substring(4); // /tr/ kısmını kaldır
            }
            
            // Query string'i kaldır
            if (path.Contains("?"))
            {
                path = path.Split('?')[0];
            }
            
            return $"{baseUrl}{path}";
        }

        /// <summary>
        /// Hreflang tag'leri oluşturur
        /// </summary>
        public static string GetHreflangTags(IUrlHelper urlHelper, string currentPath = null)
        {
            var request = urlHelper.ActionContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            
            if (string.IsNullOrEmpty(currentPath))
            {
                currentPath = request.Path.Value;
            }
            
            // Mevcut dil prefix'ini kaldır
            var cleanPath = currentPath;
            var languages = new[] { "tr", "en", "de", "fr", "es", "ru", "ar", "ja", "ko" };
            
            foreach (var lang in languages)
            {
                if (cleanPath.StartsWith($"/{lang}/"))
                {
                    cleanPath = cleanPath.Substring(4);
                    break;
                }
            }
            
            // Query string'i kaldır
            if (cleanPath.Contains("?"))
            {
                cleanPath = cleanPath.Split('?')[0];
            }
            
            var hreflangTags = new System.Text.StringBuilder();
            
            // Her dil için hreflang tag'i
            foreach (var lang in languages)
            {
                hreflangTags.AppendLine($"<link rel=\"alternate\" hreflang=\"{lang}\" href=\"{baseUrl}/{lang}{cleanPath}\" />");
            }
            
            // x-default (varsayılan)
            hreflangTags.AppendLine($"<link rel=\"alternate\" hreflang=\"x-default\" href=\"{baseUrl}{cleanPath}\" />");
            
            return hreflangTags.ToString();
        }

        /// <summary>
        /// Meta description oluşturur (max 160 karakter)
        /// </summary>
        public static string TruncateMetaDescription(string description, int maxLength = 160)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;
            
            if (description.Length <= maxLength)
                return description;
            
            // Son kelimeyi kesmemek için
            var truncated = description.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            
            if (lastSpace > 0)
            {
                truncated = truncated.Substring(0, lastSpace);
            }
            
            return truncated + "...";
        }

        /// <summary>
        /// SEO dostu URL slug oluşturur
        /// </summary>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            // Türkçe karakterleri değiştir
            text = text.ToLowerInvariant();
            text = text.Replace("ğ", "g")
                       .Replace("ü", "u")
                       .Replace("ş", "s")
                       .Replace("ı", "i")
                       .Replace("ö", "o")
                       .Replace("ç", "c");
            
            // Özel karakterleri temizle
            var chars = text.ToCharArray();
            var result = new System.Text.StringBuilder();
            
            foreach (var c in chars)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-')
                {
                    result.Append(c);
                }
                else if (c == ' ')
                {
                    result.Append('-');
                }
            }
            
            // Birden fazla tire'yi tek tire'ye çevir
            var slug = result.ToString();
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }
            
            // Başta ve sonda tire varsa kaldır
            slug = slug.Trim('-');
            
            return slug;
        }

        /// <summary>
        /// Open Graph image URL oluşturur
        /// </summary>
        public static string GetOgImageUrl(IUrlHelper urlHelper, string imagePath = null)
        {
            var request = urlHelper.ActionContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            
            if (string.IsNullOrEmpty(imagePath))
            {
                // Varsayılan logo
                return $"{baseUrl}/images/logo-moonlife1.png";
            }
            
            // Cloudinary URL ise direkt döndür
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
            {
                return imagePath;
            }
            
            // Local image ise base URL ekle
            if (imagePath.StartsWith("/"))
            {
                return $"{baseUrl}{imagePath}";
            }
            
            return $"{baseUrl}/{imagePath}";
        }
        
        /// <summary>
        /// Open Graph image URL oluşturur (default image ile)
        /// </summary>
        public static string GetOgImageUrl(IUrlHelper urlHelper, string imagePath, string defaultImagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                return GetOgImageUrl(urlHelper, imagePath);
            }
            
            return defaultImagePath;
        }
    }
}

