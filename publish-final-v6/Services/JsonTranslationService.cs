using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;

namespace MyGoldenFood.Services
{
    public interface IJsonTranslationService
    {
        string GetTranslation(string key, string language = "tr");
        Task<string> GetTranslationAsync(string key, string language = "tr");
        Task<Dictionary<string, string>> GetAllTranslationsAsync(string language = "tr");
        Task<bool> ReloadTranslationsAsync(string language = "tr");
    }

    public class JsonTranslationService : IJsonTranslationService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;
        private readonly ILogger<JsonTranslationService> _logger;
        private readonly string _translationsPath;

        public JsonTranslationService(
            IWebHostEnvironment environment,
            IMemoryCache cache,
            ILogger<JsonTranslationService> logger)
        {
            _environment = environment;
            _cache = cache;
            _logger = logger;
            _translationsPath = Path.Combine(_environment.WebRootPath, "translations");
        }

        public string GetTranslation(string key, string language = "tr")
        {
            try
            {
                var cacheKey = $"translations_{language}";
                var translations = _cache.Get<Dictionary<string, string>>(cacheKey);

                if (translations == null)
                {
                    translations = LoadTranslationsFromFile(language);
                    if (translations != null)
                    {
                        _cache.Set(cacheKey, translations, TimeSpan.FromHours(1));
                    }
                }

                if (translations != null && translations.TryGetValue(key, out var translation))
                {
                    return translation;
                }

                // Fallback to Turkish if translation not found
                if (language != "tr")
                {
                    return GetTranslation(key, "tr");
                }

                return key; // Return key if no translation found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting translation for key: {Key}, language: {Language}", key, language);
                return key;
            }
        }

        public async Task<string> GetTranslationAsync(string key, string language = "tr")
        {
            return await Task.FromResult(GetTranslation(key, language));
        }

        public async Task<Dictionary<string, string>> GetAllTranslationsAsync(string language = "tr")
        {
            try
            {
                var cacheKey = $"translations_{language}";
                var translations = _cache.Get<Dictionary<string, string>>(cacheKey);

                if (translations == null)
                {
                    translations = await LoadTranslationsFromFileAsync(language);
                    if (translations != null)
                    {
                        _cache.Set(cacheKey, translations, TimeSpan.FromHours(1));
                    }
                }

                return translations ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all translations for language: {Language}", language);
                return new Dictionary<string, string>();
            }
        }

        public async Task<bool> ReloadTranslationsAsync(string language = "tr")
        {
            try
            {
                var cacheKey = $"translations_{language}";
                _cache.Remove(cacheKey);

                var translations = await LoadTranslationsFromFileAsync(language);
                if (translations != null)
                {
                    _cache.Set(cacheKey, translations, TimeSpan.FromHours(1));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading translations for language: {Language}", language);
                return false;
            }
        }

        private Dictionary<string, string> LoadTranslationsFromFile(string language)
        {
            try
            {
                var filePath = Path.Combine(_translationsPath, $"{language}.json");
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Translation file not found: {FilePath}", filePath);
                    return null;
                }

                var jsonContent = File.ReadAllText(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                return translations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading translations from file for language: {Language}", language);
                return null;
            }
        }

        private async Task<Dictionary<string, string>> LoadTranslationsFromFileAsync(string language)
        {
            try
            {
                var filePath = Path.Combine(_translationsPath, $"{language}.json");
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Translation file not found: {FilePath}", filePath);
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                return translations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading translations from file for language: {Language}", language);
                return null;
            }
        }
    }
} 