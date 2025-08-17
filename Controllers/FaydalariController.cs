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

            var benefits = await _context.Benefits.ToListAsync();

            foreach (var benefit in benefits)
            {
                var translation = await _context.BenefitTranslations
                    .Where(t => t.BenefitId == benefit.Id && t.LanguageCode == selectedLanguage)
                    .FirstOrDefaultAsync();

                if (translation != null)
                {
                    benefit.Name = translation.Name;
                    benefit.Content = translation.Content;
                }
            }

            return View(benefits);
        }

        [HttpGet]
        public async Task<IActionResult> BenefitList()
        {
            var benefits = await _context.Benefits.ToListAsync();
            return PartialView("_BenefitListPartial", benefits);
        }

        [HttpGet]
        public IActionResult Create()
        {
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

            return PartialView("_CreateBenefitPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var benefit = await _context.Benefits.FindAsync(id);
            if (benefit == null) return NotFound();

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
    }
}
