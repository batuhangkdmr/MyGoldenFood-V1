using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public BlogController(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Blog - Ana sayfa (Herkes erişebilir)
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(blogs);
        }

        // GET: Blog/Details/5 - Blog detay sayfası (Herkes erişebilir)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.IsPublished);

            if (blog == null)
            {
                return NotFound();
            }

            // Akıllı görüntülenme sayacı - Session tabanlı
            var sessionKey = $"BlogView_{id}";
            var hasViewed = HttpContext.Session.GetString(sessionKey);
            
            if (string.IsNullOrEmpty(hasViewed))
            {
                // İlk kez görüntüleniyor - sayacı artır
                blog.ViewCount++;
                await _context.SaveChangesAsync();
                
                // Session'a kaydet (24 saat geçerli)
                HttpContext.Session.SetString(sessionKey, DateTime.Now.ToString("yyyy-MM-dd"));
            }
            else
            {
                // Bugün zaten görüntülenmiş - kontrol et
                var lastViewDate = DateTime.Parse(hasViewed);
                var today = DateTime.Today;
                
                if (lastViewDate.Date < today)
                {
                    // Farklı gün - sayacı artır
                    blog.ViewCount++;
                    await _context.SaveChangesAsync();
                    
                    // Session'ı güncelle
                    HttpContext.Session.SetString(sessionKey, today.ToString("yyyy-MM-dd"));
                }
            }

            return View(blog);
        }

        // GET: Blog/Create - Blog oluşturma sayfası (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blog/Create - Blog oluşturma (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Content,Summary,Category,Tags,SeoTitle,SeoDescription,SeoKeywords,SeoUrl")] Blog blog, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Resim yükleme işlemi
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "blog");
                    if (uploadResult != null)
                    {
                        blog.ImagePath = uploadResult;
                    }
                    else
                    {
                        ModelState.AddModelError("ImagePath", "Resim yüklenirken hata oluştu.");
                        return View(blog);
                    }
                }

                blog.CreatedDate = DateTime.Now;
                blog.Author = User.Identity.Name ?? "Admin";
                blog.IsPublished = true;
                blog.ViewCount = 0;

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Blog/Edit/5 - Blog düzenleme sayfası (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Edit/5 - Blog düzenleme (Sadece Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Summary,Category,Tags,SeoTitle,SeoDescription,SeoKeywords,SeoUrl,IsPublished")] Blog blog, IFormFile imageFile)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.Blogs.FindAsync(id);
                    if (existingBlog != null)
                    {
                        // Yeni resim yüklendiyse
                        if (imageFile != null && imageFile.Length > 0)
                        {
                            // Eski resmi Cloudinary'den sil
                            if (!string.IsNullOrEmpty(existingBlog.ImagePath))
                            {
                                await _cloudinaryService.DeleteImageAsync(existingBlog.ImagePath);
                            }

                            var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, "blog");
                            if (uploadResult != null)
                            {
                                existingBlog.ImagePath = uploadResult;
                            }
                            else
                            {
                                ModelState.AddModelError("ImagePath", "Resim yüklenirken hata oluştu.");
                                return View(blog);
                            }
                        }

                        existingBlog.Title = blog.Title;
                        existingBlog.Content = blog.Content;
                        existingBlog.Summary = blog.Summary;
                        existingBlog.Category = blog.Category;
                        existingBlog.Tags = blog.Tags;
                        existingBlog.SeoTitle = blog.SeoTitle;
                        existingBlog.SeoDescription = blog.SeoDescription;
                        existingBlog.SeoKeywords = blog.SeoKeywords;
                        existingBlog.SeoUrl = blog.SeoUrl;
                        existingBlog.IsPublished = blog.IsPublished;
                        existingBlog.UpdatedDate = DateTime.Now;

                        _context.Update(existingBlog);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Blog/Delete/5 - Blog silme onay sayfası (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5 - Blog silme (Sadece Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog != null)
                {
                    // Eski resmi Cloudinary'den sil
                    if (!string.IsNullOrEmpty(blog.ImagePath))
                    {
                        await _cloudinaryService.DeleteImageAsync(blog.ImagePath);
                    }

                    _context.Blogs.Remove(blog);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Blog yazısı başarıyla silindi." });
                }

                return Json(new { success = false, message = "Blog yazısı bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // GET: Blog/Admin - Admin paneli (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(blogs);
        }

        // GET: Blog/AdminList - Admin Dashboard için sadece liste (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return PartialView("_AdminList", blogs);
        }

        // POST: Blog/TogglePublish/5 - Yayın durumunu değiştir (Sadece Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TogglePublish(int id)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog != null)
                {
                    blog.IsPublished = !blog.IsPublished;
                    blog.UpdatedDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Blog yazısının yayın durumu değiştirildi." });
                }

                return Json(new { success = false, message = "Blog yazısı bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
