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
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                    category.Description = translation.Description;
                }
            }

            // Varsayılan olarak ilk kategorideki ürünleri de getir
            var defaultBenefits = new List<Benefit>();
            if (categories.Any())
            {
                var firstCategory = categories.First();
                defaultBenefits = await _context.Benefits
                    .Include(b => b.BenefitCategory)
                    .Include(b => b.Translations)
                    .Where(b => b.BenefitCategoryId == firstCategory.Id)
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
            }

            ViewBag.Categories = categories;
            ViewBag.DefaultBenefits = defaultBenefits;
            ViewBag.SelectedCategoryId = categories.Any() ? categories.First().Id : 0;
            ViewBag.SelectedLanguage = selectedLanguage;
            ViewBag.HasCategories = categories.Any();
            ViewBag.FirstCategoryName = categories.Any() ? categories.First().Name : "";

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
        public async Task<IActionResult> GetBenefitsByCategory(int id)
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
                .Where(b => b.BenefitCategoryId == id)
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

            return PartialView("_BenefitsByCategoryPartial", benefits);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
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
                .Where(b => b.BenefitCategoryId == id)
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

            var category = await _context.BenefitCategories
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                var translatedCategory = category.Translations
                    .FirstOrDefault(t => t.Language == selectedLanguage);

                if (translatedCategory != null)
                {
                    category.Name = translatedCategory.Name;
                    category.Description = translatedCategory.Description;
                }
            }

            ViewBag.Category = category;
            return View(benefits);
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

                _context.BenefitCategories.Add(model);
                await _context.SaveChangesAsync();

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedDescription = await translationService.TranslateText(model.Description, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedDescription))
                    {
                        var newTranslation = new BenefitCategoryTranslation
                        {
                            BenefitCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName,
                            Description = translatedDescription
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
                existingCategory.Description = model.Description;

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
                    var translatedDescription = await translationService.TranslateText(model.Description, lang, "tr");

                    var translation = await _context.BenefitCategoryTranslations.FirstOrDefaultAsync(t =>
                        t.BenefitCategoryId == model.Id && t.Language == lang);

                    if (translation != null)
                    {
                        translation.Name = !string.IsNullOrEmpty(translatedName) ? translatedName : translation.Name;
                        translation.Description = !string.IsNullOrEmpty(translatedDescription) ? translatedDescription : translation.Description;
                    }
                    else if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedDescription))
                    {
                        _context.BenefitCategoryTranslations.Add(new BenefitCategoryTranslation
                        {
                            BenefitCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName,
                            Description = translatedDescription
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
                .ToListAsync();
            return PartialView("_BenefitCategoryListPartial", categories);
        }
    }
}
