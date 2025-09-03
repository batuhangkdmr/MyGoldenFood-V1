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
                .Include(c => c.Recipes)
                .Include(c => c.ChildCategories)
                    .ThenInclude(child => child.Recipes)
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

            // Varsayılan olarak tüm tarifleri getir
            var defaultRecipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Include(r => r.RecipeTranslations)
                .ToListAsync();

            foreach (var recipe in defaultRecipes)
            {
                var translation = recipe.RecipeTranslations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            ViewBag.Categories = categories;
            ViewBag.DefaultRecipes = defaultRecipes;
            ViewBag.SelectedCategoryId = 0; // 0 = Tüm Kategoriler
            ViewBag.SelectedLanguage = selectedLanguage;
            ViewBag.HasCategories = categories.Any();
            ViewBag.FirstCategoryName = "Tüm Kategoriler";
            ViewBag.CloudinaryService = _cloudinaryService; // 🚀 Responsive resimler için

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> RecipeCategoryList()
        {
            var categories = await _context.RecipeCategories
                .Include(c => c.Translations)
                .ToListAsync();
            return PartialView("_RecipeCategoryListPartial", categories);
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
        public async Task<IActionResult> CreateCategory()
        {
            return PartialView("_CreateRecipeCategoryPartial");
        }

        [HttpGet]
        public async Task<IActionResult> GetParentCategories()
        {
            var parentCategories = await _context.RecipeCategories
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

        [HttpPost]
        public async Task<IActionResult> CreateCategory(RecipeCategory model, IFormFile ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipe-categories");
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

                _context.RecipeCategories.Add(model);
                await _context.SaveChangesAsync();

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");

                    if (!string.IsNullOrEmpty(translatedName))
                    {
                        var newTranslation = new RecipeCategoryTranslation
                        {
                            RecipeCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName
                        };
                        _context.RecipeCategoryTranslations.Add(newTranslation);
                    }
                }

                await _context.SaveChangesAsync();
                await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
                return Json(new { success = true, message = "Tarif kategorisi başarıyla eklendi!" });
            }

            return PartialView("_CreateRecipeCategoryPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.RecipeCategories.FindAsync(id);
            if (category == null) return NotFound();

            return PartialView("_EditRecipeCategoryPartial", category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(RecipeCategory model, IFormFile? ImageFile, [FromServices] DeepLTranslationService translationService)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _context.RecipeCategories.FindAsync(model.Id);
                if (existingCategory == null) return NotFound();

                existingCategory.Name = model.Name;
                existingCategory.SortOrder = model.SortOrder;
                
                // Alt kategori sistemi için Level ve ParentCategoryId ayarla
                if (model.ParentCategoryId.HasValue)
                {
                    existingCategory.Level = 1; // Alt kategori
                    existingCategory.ParentCategoryId = model.ParentCategoryId;
                }
                else
                {
                    existingCategory.Level = 0; // Ana kategori
                    existingCategory.ParentCategoryId = null;
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    await _cloudinaryService.DeleteImageAsync(existingCategory.ImagePath);
                    var uploadResult = await _cloudinaryService.UploadImageAsync(ImageFile, "recipe-categories");
                    if (uploadResult != null)
                    {
                        existingCategory.ImagePath = uploadResult;
                    }
                }

                string[] languages = { "en", "de", "fr", "ru", "ja", "ko", "ar" };

                foreach (var lang in languages)
                {
                    var translatedName = await translationService.TranslateText(model.Name, lang, "tr");

                    var translation = await _context.RecipeCategoryTranslations.FirstOrDefaultAsync(t =>
                        t.RecipeCategoryId == model.Id && t.Language == lang);

                    if (translation != null)
                    {
                        translation.Name = !string.IsNullOrEmpty(translatedName) ? translatedName : translation.Name;
                    }
                    else if (!string.IsNullOrEmpty(translatedName))
                    {
                        _context.RecipeCategoryTranslations.Add(new RecipeCategoryTranslation
                        {
                            RecipeCategoryId = model.Id,
                            Language = lang,
                            Name = translatedName
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
                return Json(new { success = true, message = "Tarif kategorisi başarıyla güncellendi!" });
            }

            return PartialView("_EditRecipeCategoryPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.RecipeCategories
                .Include(c => c.Recipes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı!" });
            }

            if (category.Recipes.Any())
            {
                return Json(new { success = false, message = "Bu kategoride tarifler bulunduğu için silinemez!" });
            }

            if (!string.IsNullOrEmpty(category.ImagePath))
            {
                await _cloudinaryService.DeleteImageAsync(category.ImagePath);
            }

            _context.RecipeCategories.Remove(category);

            var translations = await _context.RecipeCategoryTranslations
                .Where(t => t.RecipeCategoryId == id)
                .ToListAsync();
            _context.RecipeCategoryTranslations.RemoveRange(translations);

            await _context.SaveChangesAsync();
            await _tariflerHubContext.Clients.All.SendAsync("TarifUpdated");
            return Json(new { success = true, message = "Tarif kategorisi başarıyla silindi!" });
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

        [HttpGet]
        public async Task<IActionResult> GetAllRecipes()
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Include(r => r.RecipeTranslations)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                var translation = recipe.RecipeTranslations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            var result = recipes.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                content = r.Content,
                imagePath = r.ImagePath,
                recipeCategoryId = r.RecipeCategoryId,
                recipeCategoryName = r.RecipeCategory?.Name ?? "Kategori Yok"
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipesByCategory(int id)
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
                categoryIds = await _context.RecipeCategories.Select(c => c.Id).ToListAsync();
            }
            else
            {
                var category = await _context.RecipeCategories
                    .Include(c => c.ChildCategories)
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

            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Include(r => r.RecipeTranslations)
                .Where(r => id == 0 || categoryIds.Contains(r.RecipeCategoryId))
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                var translation = recipe.RecipeTranslations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            var result = recipes.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                content = r.Content,
                imagePath = r.ImagePath,
                recipeCategoryId = r.RecipeCategoryId,
                recipeCategoryName = r.RecipeCategory?.Name ?? "Kategori Yok"
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> TestSubcategoryRecipes(int categoryId)
        {
            // Debug için alt kategori tariflerini test et
            var category = await _context.RecipeCategories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
            
            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Where(r => r.RecipeCategoryId == categoryId)
                .ToListAsync();
            
            var result = new
            {
                categoryId = categoryId,
                categoryName = category?.Name,
                categoryLevel = category?.Level,
                parentCategoryId = category?.ParentCategoryId,
                isSubcategory = category?.Level == 1,
                recipeCount = recipes.Count,
                recipes = recipes.Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    categoryId = r.RecipeCategoryId,
                    categoryName = r.RecipeCategory?.Name
                }).ToList()
            };
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipesBySubcategory(int recipeCategoryId)
        {
            string selectedLanguage = "tr";

            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            // Alt kategorideki tarifleri direkt getir
            var recipes = await _context.Recipes
                .Include(r => r.RecipeCategory)
                .Include(r => r.RecipeTranslations)
                .Where(r => r.RecipeCategoryId == recipeCategoryId)
                .ToListAsync();

            // Debug: Kaç tarif bulundu
            Console.WriteLine($"GetRecipesBySubcategory - recipeCategoryId: {recipeCategoryId}, bulunan tarif sayısı: {recipes.Count}");

            foreach (var recipe in recipes)
            {
                var translation = recipe.RecipeTranslations.FirstOrDefault(t => t.LanguageCode == selectedLanguage);
                if (translation != null)
                {
                    recipe.Name = translation.Name;
                    recipe.Content = translation.Content;
                }
            }

            var result = recipes.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                content = r.Content,
                imagePath = r.ImagePath,
                recipeCategoryId = r.RecipeCategoryId,
                recipeCategoryName = r.RecipeCategory?.Name ?? "Kategori Yok"
            }).ToList();

            return Json(result);
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

            var categories = await _context.RecipeCategories
                .Include(c => c.Translations)
                .Include(c => c.ChildCategories)
                .Include(c => c.Recipes)
                .Where(c => c.Level == 0) // Sadece ana kategoriler
                .OrderBy(c => c.SortOrder)
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
                sortOrder = c.SortOrder,
                recipeCount = c.Recipes.Count + c.ChildCategories.Sum(child => child.Recipes.Count),
                childCategories = c.ChildCategories.OrderBy(child => child.SortOrder).ThenBy(child => child.Name).Select(child => new
                {
                    id = child.Id,
                    name = child.Name,
                    imagePath = child.ImagePath,
                    recipeCount = child.Recipes.Count
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

            var subCategories = await _context.RecipeCategories
                .Include(c => c.Translations)
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
                recipeCount = c.Recipes.Count
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryInfo(int categoryId)
        {
            var category = await _context.RecipeCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return Json(new { error = "Kategori bulunamadı" });
            }

            var result = new
            {
                id = category.Id,
                name = category.Name,
                isChild = category.Level == 1,
                parentId = category.ParentCategoryId
            };

            return Json(result);
        }
    }
}
