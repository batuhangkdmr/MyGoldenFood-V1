using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Hubs;
using MyGoldenFood.Models;
using MyGoldenFood.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyGoldenFood.Controllers
{
    public class FaydalariController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IHubContext<FaydalariHub> _faydalariHubContext;

        public FaydalariController(AppDbContext context, CloudinaryService cloudinaryService, IHubContext<FaydalariHub> faydalariHubContext)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _faydalariHubContext = faydalariHubContext;
        }

        // SEO Slug oluşturma helper metodu
        private string GenerateSeoSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Türkçe karakterleri değiştir
            title = title.ToLower()
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u");

            // Özel karakterleri kaldır ve tire ile değiştir
            title = System.Text.RegularExpressions.Regex.Replace(title, @"[^a-z0-9\s-]", "");
            title = System.Text.RegularExpressions.Regex.Replace(title, @"\s+", "-");
            title = title.Trim('-');

            // Duplicate slug'ları önlemek için ID ekle
            var baseSlug = title;
            var counter = 1;
            var finalSlug = baseSlug;

            // Aynı slug'ın var olup olmadığını kontrol et
            while (_context.Benefits.Any(b => b.SeoSlug == finalSlug))
            {
                finalSlug = $"{baseSlug}-{counter}";
                counter++;
            }

            return finalSlug;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var categories = await _context.BenefitCategories
                .Include(c => c.Translations)
                .Include(c => c.Benefits)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Benefits)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Translations)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                }

                // Alt kategorilerin çevirilerini de güncelle
                foreach (var child in category.ChildCategories)
                {
                    var childTranslation = child.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                    if (childTranslation != null)
                    {
                        child.Name = childTranslation.Name;
                    }
                }
            }

            // Varsayılan olarak tüm faydaları getir
            var defaultBenefits = await _context.Benefits
                .Include(b => b.BenefitCategory)
                .Include(b => b.Translations)
                .ToListAsync();

            foreach (var benefit in defaultBenefits)
            {
                var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            ViewBag.Categories = categories;
            ViewBag.DefaultBenefits = defaultBenefits;
            ViewBag.SelectedCategoryId = 0; // 0 = Tüm Kategoriler
            ViewBag.SelectedLanguage = selectedLanguage;
            ViewBag.HasCategories = categories.Any();
            ViewBag.FirstCategoryName = "Tüm Kategoriler";
            ViewBag.CloudinaryService = _cloudinaryService; // 🚀 Responsive resimler için

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BenefitList()
        {
            var benefits = await _context.Benefits
                .Include(b => b.BenefitCategory)
                .ToListAsync();
            return PartialView("_BenefitListPartial", benefits);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBenefits()
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var benefits = await _context.Benefits
                .Include(b => b.BenefitCategory)
                .Include(b => b.Translations)
                .ToListAsync();

            foreach (var benefit in benefits)
            {
                var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            // JSON formatında döndür
            var benefitData = benefits.Select(b => new
            {
                name = b.Name,
                content = b.Content,
                imagePath = b.ImagePath
            });

            return Json(benefitData);
        }

        [HttpGet]
        public async Task<IActionResult> GetBenefitsByCategory(int id)
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var categoryIds = new List<int>();
            
            if (id == 0) // Tüm Kategoriler
            {
                // Tüm kategorilerin ID'lerini al
                categoryIds = await _context.BenefitCategories.Select(c => c.Id).ToListAsync();
            }
            else
            {
                var category = await _context.BenefitCategories
                    .Include(c => c.ChildCategories)
                        .ThenInclude(child => child.Translations)
                    .Include(c => c.Translations)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (category != null)
                {
                    if (category.Level == 0) // Ana kategori (ParentCategoryId = NULL)
                    {
                        // Ana kategori seçilirse, alt kategorilerin ID'lerini de ekle
                        categoryIds.Add(id);
                        if (category.ChildCategories.Any())
                        {
                            categoryIds.AddRange(category.ChildCategories.Select(c => c.Id));
                        }
                    }
                    else // Alt kategori (ParentCategoryId != NULL)
                    {
                        // Alt kategori seçilirse, sadece o alt kategorinin ID'sini kullan
                        categoryIds.Add(id);
                    }
                }
            }

            var benefits = await _context.Benefits
                .Include(b => b.BenefitCategory)
                    .ThenInclude(c => c.Translations)
                .Include(b => b.Translations)
                .Where(b => id == 0 || categoryIds.Contains(b.BenefitCategoryId ?? 0))
                .ToListAsync();

            foreach (var benefit in benefits)
            {
                var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            // JSON formatında döndür
            var benefitData = benefits.Select(b => new
            {
                id = b.Id,
                name = b.Name,
                content = b.Content,
                imagePath = b.ImagePath,
                benefitCategoryId = b.BenefitCategoryId,
                benefitCategoryName = b.BenefitCategory?.Name ?? "Kategori Yok",
                seoSlug = b.SeoSlug,
                seoTitle = b.SeoTitle,
                seoDescription = b.SeoDescription,
                preparationTime = b.PreparationTime,
                cookingTime = b.CookingTime,
                servings = b.Servings,
                difficulty = b.Difficulty
            });

            return Json(benefitData);
        }

        [HttpGet]
        public async Task<IActionResult> GetBenefitsBySubcategory(int benefitCategoryId)
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            // Alt kategorideki faydaları direkt getir
            var benefits = await _context.Benefits
                .Include(b => b.BenefitCategory)
                .Include(b => b.Translations)
                .Where(b => b.BenefitCategoryId == benefitCategoryId)
                .ToListAsync();

            // Debug: Kaç fayda bulundu
            Console.WriteLine($"GetBenefitsBySubcategory - benefitCategoryId: {benefitCategoryId}, bulunan fayda sayısı: {benefits.Count}");

            foreach (var benefit in benefits)
            {
                var translation = benefit.Translations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            var result = benefits.Select(b => new
            {
                id = b.Id,
                name = b.Name,
                content = b.Content,
                imagePath = b.ImagePath,
                benefitCategoryId = b.BenefitCategoryId,
                benefitCategoryName = b.BenefitCategory?.Name ?? "Kategori Yok",
                seoSlug = b.SeoSlug,
                seoTitle = b.SeoTitle,
                seoDescription = b.SeoDescription,
                preparationTime = b.PreparationTime,
                cookingTime = b.CookingTime,
                servings = b.Servings,
                difficulty = b.Difficulty
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryInfo(int categoryId)
        {
            var category = await _context.BenefitCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return Json(new { level = 0, parentId = (int?)null });
            }

            return Json(new { 
                level = category.Level, 
                parentId = category.ParentCategoryId 
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.BenefitCategories.ToList();
            return PartialView("_CreateBenefitPartial");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Benefit model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefits");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                // Otomatik SEO alanlarını oluştur
                model.SeoTitle = model.Name;
                model.SeoDescription = model.Content?.Length > 160 ? model.Content.Substring(0, 157) + "..." : model.Content;
                model.SeoKeywords = $"fayda, {model.Name.ToLower()}, sağlık, beslenme";
                model.SeoSlug = GenerateSeoSlug(model.Name);
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;

                _context.Benefits.Add(model);
                await _context.SaveChangesAsync();

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                    {
                        var newTranslation = new BenefitTranslation
                        {
                            BenefitId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.BenefitTranslations.Add(newTranslation);
                    }
                }

                await _context.SaveChangesAsync();
                await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
                return Json(new { success = true, message = "Fayda başarıyla eklendi!" });
            }

            ViewBag.Categories = _context.BenefitCategories.ToList();
            return PartialView("_CreateBenefitPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null) return NotFound();

            ViewBag.Categories = _context.BenefitCategories.ToList();
            return PartialView("_EditBenefitPartial", benefit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Benefit model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                var existingBenefit = await _context.Benefits.FindAsync(model.Id);
                if (existingBenefit == null) return NotFound();

                existingBenefit.Name = model.Name;
                existingBenefit.Content = model.Content;
                existingBenefit.BenefitCategoryId = model.BenefitCategoryId;

                // SEO alanlarını güncelle
                existingBenefit.SeoTitle = model.Name;
                existingBenefit.SeoDescription = model.Content?.Length > 160 ? model.Content.Substring(0, 157) + "..." : model.Content;
                existingBenefit.SeoKeywords = $"fayda, {model.Name.ToLower()}, sağlık, beslenme";
                existingBenefit.SeoSlug = GenerateSeoSlug(model.Name);
                existingBenefit.UpdatedDate = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    await _cloudinaryService.DeleteImageAsync(existingBenefit.ImagePath);
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefits");
                    if (uploadResult != null)
                    {
                        existingBenefit.ImagePath = uploadResult;
                    }
                }

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                    var nameTranslation = await _context.BenefitTranslations.FirstOrDefaultAsync(t =>
                        t.BenefitId == model.Id && t.LanguageCode == lang);

                    if (nameTranslation != null)
                    {
                        nameTranslation.Name = !string.IsNullOrEmpty(translatedName) ? translatedName : nameTranslation.Name;
                        nameTranslation.Content = !string.IsNullOrEmpty(translatedContent) ? translatedContent : nameTranslation.Content;
                    }
                    else if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                    {
                        _context.BenefitTranslations.Add(new BenefitTranslation
                        {
                            BenefitId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
                return Json(new { success = true, message = "Fayda başarıyla güncellendi!" });
            }

            ViewBag.Categories = _context.BenefitCategories.ToList();
            return PartialView("_EditBenefitPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null)
            {
                return Json(new { success = false, message = "Fayda bulunamadı!" });
            }

            if (!string.IsNullOrEmpty(benefit.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(benefit.ImagePath);
            }

            _context.Benefits.Remove(benefit);

            var translations = await _context.BenefitTranslations
                .Where(t => t.BenefitId == id)
                .ToListAsync();
            _context.BenefitTranslations.RemoveRange(translations);

            await _context.SaveChangesAsync();
            await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
            return Json(new { success = true, message = "Fayda başarıyla silindi!" });
        }

        // Kategori CRUD İşlemleri
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return PartialView("_CreateBenefitCategoryPartial");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(BenefitCategory model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefit-categories");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                // Alt kategori sistemi için Level ve ParentCategoryId ayarla
                if (model.ParentCategoryId.HasValue)
                {
                    model.Level = 1; // Alt kategori
                }
                else
                {
                    model.Level = 0; // Ana kategori
                    model.ParentCategoryId = null;
                }

                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;

                _context.BenefitCategories.Add(model);
                await _context.SaveChangesAsync();

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName))
                    {
                        var newTranslation = new BenefitCategoryTranslation
                        {
                            BenefitCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName
                        };
                        _context.BenefitCategoryTranslations.Add(newTranslation);
                    }
                }

                await _context.SaveChangesAsync();
                await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
                return Json(new { success = true, message = "Fayda kategorisi başarıyla eklendi!" });
            }

            return PartialView("_CreateBenefitCategoryPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.BenefitCategories.FindAsync(id);
            if (category == null) return NotFound();

            return PartialView("_EditBenefitCategoryPartial", category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(BenefitCategory model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _context.BenefitCategories.FindAsync(model.Id);
                if (existingCategory == null) return NotFound();

                existingCategory.Name = model.Name;
                existingCategory.ParentCategoryId = model.ParentCategoryId;
                existingCategory.SortOrder = model.SortOrder;

                // Level'ı güncelle
                if (model.ParentCategoryId.HasValue)
                {
                    existingCategory.Level = 1; // Alt kategori
                }
                else
                {
                    existingCategory.Level = 0; // Ana kategori
                    existingCategory.ParentCategoryId = null;
                }

                existingCategory.UpdatedDate = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    await _cloudinaryService.DeleteImageAsync(existingCategory.ImagePath);
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "benefit-categories");
                    if (uploadResult != null)
                    {
                        existingCategory.ImagePath = uploadResult;
                    }
                }

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");

                    var translation = await _context.BenefitCategoryTranslations.FirstOrDefaultAsync(t =>
                        t.BenefitCategoryId == model.Id && t.Language == lang);

                    if (translation != null)
                    {
                        translation.Name = !string.IsNullOrEmpty(translatedName) ? translatedName : translation.Name;
                    }
                    else if (!string.IsNullOrEmpty(translatedName))
                    {
                        _context.BenefitCategoryTranslations.Add(new BenefitCategoryTranslation
                        {
                            BenefitCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
                return Json(new { success = true, message = "Fayda kategorisi başarıyla güncellendi!" });
            }

            return PartialView("_EditBenefitCategoryPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.BenefitCategories
                .Include(c => c.Benefits)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı!" });
            }

            if (category.Benefits.Any())
            {
                return Json(new { success = false, message = "Bu kategoride faydalar bulunduğu için silinemez!" });
            }

            if (!string.IsNullOrEmpty(category.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(category.ImagePath);
            }

            _context.BenefitCategories.Remove(category);

            var translations = await _context.BenefitCategoryTranslations
                .Where(t => t.BenefitCategoryId == id)
                .ToListAsync();
            _context.BenefitCategoryTranslations.RemoveRange(translations);

            await _context.SaveChangesAsync();
            await _faydalariHubContext.Clients.All.SendAsync("BenefitUpdated");
            return Json(new { success = true, message = "Fayda kategorisi başarıyla silindi!" });
        }

        [HttpGet]
        public async Task<IActionResult> CategoryList()
        {
            var categories = await _context.BenefitCategories
                .Include(c => c.Benefits)
                .Include(c => c.ChildCategories)
                .ToListAsync();
            return PartialView("_BenefitCategoryListPartial", categories);
        }

        // Alt kategori sistemi için yeni metodlar
        [HttpGet]
        public async Task<IActionResult> GetParentCategories()
        {
            var parentCategories = await _context.BenefitCategories
                .Where(c => c.Level == 0) // Sadece ana kategoriler
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
                .ToListAsync();

            return Json(parentCategories);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesWithChildren()
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var categories = await _context.BenefitCategories
                .Include(c => c.Translations)
                .Include(c => c.Benefits)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Benefits)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Translations)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                }

                // Alt kategorilerin çevirilerini de güncelle
                foreach (var child in category.ChildCategories)
                {
                    var childTranslation = child.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                    if (childTranslation != null)
                    {
                        child.Name = childTranslation.Name;
                    }
                }
            }

            var result = categories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                imagePath = c.ImagePath,
                level = c.Level,
                parentCategoryId = c.ParentCategoryId,
                sortOrder = c.SortOrder,
                benefitsCount = c.Benefits?.Count ?? 0,
                childCategories = c.ChildCategories.Select(child => new
                {
                    id = child.Id,
                    name = child.Name,
                    imagePath = child.ImagePath,
                    benefitsCount = child.Benefits?.Count ?? 0
                }).ToList()
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var subCategories = await _context.BenefitCategories
                .Include(c => c.Translations)
                .Include(c => c.Benefits)
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            foreach (var category in subCategories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                }
            }

            var result = subCategories.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                imagePath = c.ImagePath,
                benefitsCount = c.Benefits?.Count ?? 0
            }).ToList();

            return Json(result);
        }

    }
}
