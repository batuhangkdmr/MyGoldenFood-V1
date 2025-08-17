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
    public class TariflerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IHubContext<TariflerHub> _tariflerHubContext;
        
        public TariflerController(AppDbContext context, CloudinaryService cloudinaryService, IHubContext<TariflerHub> tariflerHubContext)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _tariflerHubContext = tariflerHubContext;
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

            var categories = await _context.RecipeCategories
                .Include(c => c.Translations)
                .ToListAsync();

            foreach (var category in categories)
            {
                var translation = category.Translations.FirstOrDefault(t => t.Language == selectedLanguage);
                if (translation != null)
                {
                    category.Name = translation.Name;
                }
            }

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> RecipeCategoryList()
        {
            var categories = await _context.RecipeCategories.ToListAsync();
            return PartialView("_RecipeListPartial", categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_CreateRecipePartial");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Recipe model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipes");
                    if (uploadResult != null)
                    {
                        model.ImagePath = uploadResult;
                    }
                }

                _context.Recipes.Add(model);
                await _context.SaveChangesAsync();

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                    {
                        var newTranslation = new RecipeTranslation
                        {
                            RecipeId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        };
                        _context.RecipeTranslations.Add(newTranslation);
                    }
                }

                await _context.SaveChangesAsync();
                await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
                return Json(new { success = true, message = "Tarif başarıyla eklendi!" });
            }

            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_CreateRecipePartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> RecipeList(int categoryId)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Select(r => new
                {
                    Id = r.Id,
                    Name = r.Name,
                    Content = r.Content,
                    ImagePath = r.ImagePath,
                    RecipeCategoryId = r.RecipeCategoryId,
                    RecipeCategoryName = r.RecipeCategory != null ? r.RecipeCategory.Name : "Kategori Yok"
                })
                .ToListAsync();

            var recipeModels = recipes.Select(r => new Recipe
            {
                Id = r.Id,
                Name = r.Name,
                Content = r.Content,
                ImagePath = r.ImagePath,
                RecipeCategoryId = r.RecipeCategoryId,
                RecipeCategory = new RecipeCategory { Name = r.RecipeCategoryName }
            }).ToList();

            return PartialView("_RecipeListPartial", recipeModels);
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

            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Where(r => r.RecipeCategoryId == id)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                var translation = await _context.RecipeTranslations
                    .Where(t => t.RecipeId == recipe.Id && t.LanguageCode == selectedLanguage)
                    .FirstOrDefaultAsync();

                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            var category = await _context.RecipeCategories
                .Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                var translatedCategory = category.Translations
                    .FirstOrDefault(t => t.Language == selectedLanguage);

                if (translatedCategory != null)
                {
                    category.Name = translatedCategory.Name;
                }
            }

            ViewBag.Category = category;
            return View(recipes);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_EditRecipePartial", recipe);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Recipe model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                var existingRecipe = await _context.Recipes.FindAsync(model.Id);
                if (existingRecipe == null) return NotFound();

                existingRecipe.Name = model.Name;
                existingRecipe.Content = model.Content;
                existingRecipe.RecipeCategoryId = model.RecipeCategoryId;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    await _cloudinaryService.DeleteImageAsync(existingRecipe.ImagePath);
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipes");
                    if (uploadResult != null)
                    {
                        existingRecipe.ImagePath = uploadResult;
                    }
                }

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");
                    var translatedContent = await translationService.TranslateText(model.Content, lang, "tr");

                    var nameTranslation = await _context.RecipeTranslations.FirstOrDefaultAsync(t =>
                        t.RecipeId == model.Id && t.LanguageCode == lang);

                    if (nameTranslation != null)
                    {
                        nameTranslation.Name = !string.IsNullOrEmpty(translatedName) ? translatedName : nameTranslation.Name;
                        nameTranslation.Content = !string.IsNullOrEmpty(translatedContent) ? translatedContent : nameTranslation.Content;
                    }
                    else if (!string.IsNullOrEmpty(translatedName) && !string.IsNullOrEmpty(translatedContent))
                    {
                        _context.RecipeTranslations.Add(new RecipeTranslation
                        {
                            RecipeId = model.Id,
                            LanguageCode = lang,
                            Name = translatedName,
                            Content = translatedContent
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
                return Json(new { success = true, message = "Tarif başarıyla güncellendi!" });
            }

            ViewBag.Categories = _context.RecipeCategories.ToList();
            return PartialView("_EditRecipePartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return Json(new { success = false, message = "Tarif bulunamadı!" });
            }

            if (!string.IsNullOrEmpty(recipe.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(recipe.ImagePath);
            }

            _context.Recipes.Remove(recipe);

            var translations = await _context.RecipeTranslations
                .Where(t => t.RecipeId == id)
                .ToListAsync();
            _context.RecipeTranslations.RemoveRange(translations);

            await _context.SaveChangesAsync();
            await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
            return Json(new { success = true, message = "Tarif başarıyla silindi!" });
        }
    }
}
