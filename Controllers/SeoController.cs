using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyGoldenFood.Controllers
{
    public class SeoController : Controller
    {
        private readonly AppDbContext _context;

        public SeoController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// robots.txt dosyasını döndürür
        /// </summary>
        [Route("robots.txt")]
        [HttpGet]
        public IActionResult RobotsTxt()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var robotsContent = new StringBuilder();
            
            robotsContent.AppendLine("User-agent: *");
            robotsContent.AppendLine("Allow: /");
            robotsContent.AppendLine("Disallow: /Admin/");
            robotsContent.AppendLine("Disallow: /bin/");
            robotsContent.AppendLine("Disallow: /obj/");
            robotsContent.AppendLine("Disallow: /publish-*/");
            robotsContent.AppendLine("Disallow: /*.json$");
            robotsContent.AppendLine("Disallow: /Hub/");
            robotsContent.AppendLine("");
            robotsContent.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
            robotsContent.AppendLine($"Sitemap: {baseUrl}/sitemap-products.xml");
            robotsContent.AppendLine($"Sitemap: {baseUrl}/sitemap-recipes.xml");
            robotsContent.AppendLine($"Sitemap: {baseUrl}/sitemap-blog.xml");

            return Content(robotsContent.ToString(), "text/plain", Encoding.UTF8);
        }

        /// <summary>
        /// Ana XML Sitemap - Tüm sayfaların listesi
        /// </summary>
        [Route("sitemap.xml")]
        [HttpGet]
        public async Task<IActionResult> Sitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var currentDate = DateTime.UtcNow;

            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("sitemapindex",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XElement("sitemap",
                        new XElement("loc", $"{baseUrl}/sitemap-pages.xml"),
                        new XElement("lastmod", currentDate.ToString("yyyy-MM-dd"))
                    ),
                    new XElement("sitemap",
                        new XElement("loc", $"{baseUrl}/sitemap-products.xml"),
                        new XElement("lastmod", currentDate.ToString("yyyy-MM-dd"))
                    ),
                    new XElement("sitemap",
                        new XElement("loc", $"{baseUrl}/sitemap-recipes.xml"),
                        new XElement("lastmod", currentDate.ToString("yyyy-MM-dd"))
                    ),
                    new XElement("sitemap",
                        new XElement("loc", $"{baseUrl}/sitemap-blog.xml"),
                        new XElement("lastmod", currentDate.ToString("yyyy-MM-dd"))
                    )
                )
            );

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Statik sayfalar için sitemap
        /// </summary>
        [Route("sitemap-pages.xml")]
        [HttpGet]
        public IActionResult SitemapPages()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var currentDate = DateTime.UtcNow;

            var pages = new[]
            {
                new { Url = "", Priority = "1.0", ChangeFreq = "daily" }, // Ana sayfa
                new { Url = "/urunler", Priority = "0.9", ChangeFreq = "weekly" },
                new { Url = "/tarifler", Priority = "0.9", ChangeFreq = "weekly" },
                new { Url = "/faydalari", Priority = "0.8", ChangeFreq = "monthly" },
                new { Url = "/tariflervefaydalari", Priority = "0.8", ChangeFreq = "weekly" },
                new { Url = "/hakkimizda", Priority = "0.7", ChangeFreq = "monthly" },
                new { Url = "/iletisim", Priority = "0.7", ChangeFreq = "monthly" },
                new { Url = "/Bayilik", Priority = "0.6", ChangeFreq = "monthly" },
                new { Url = "/Blog", Priority = "0.8", ChangeFreq = "daily" },
                // SEO Landing Pages
                new { Url = "/seo/donuk-gida-nedir", Priority = "0.8", ChangeFreq = "monthly" }
            };

            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute(XNamespace.Xmlns + "xhtml", "http://www.w3.org/1999/xhtml"),
                    pages.Select(page => new XElement("url",
                        new XElement("loc", $"{baseUrl}{page.Url}"),
                        new XElement("lastmod", currentDate.ToString("yyyy-MM-dd")),
                        new XElement("changefreq", page.ChangeFreq),
                        new XElement("priority", page.Priority),
                        // Hreflang alternatifleri (9 dil)
                        GetHreflangAlternatives(baseUrl, page.Url)
                    ))
                )
            );

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Ürünler için sitemap
        /// </summary>
        [Route("sitemap-products.xml")]
        [HttpGet]
        public async Task<IActionResult> SitemapProducts()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var products = await _context.Products
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute(XNamespace.Xmlns + "xhtml", "http://www.w3.org/1999/xhtml"),
                    products.Select(product => new XElement("url",
                        new XElement("loc", $"{baseUrl}/urunler"),
                        new XElement("lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd")),
                        new XElement("changefreq", "weekly"),
                        new XElement("priority", "0.8"),
                        GetHreflangAlternatives(baseUrl, "/urunler")
                    ))
                )
            );

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Tarifler için sitemap
        /// </summary>
        [Route("sitemap-recipes.xml")]
        [HttpGet]
        public async Task<IActionResult> SitemapRecipes()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var recipes = await _context.Recipes
                .OrderByDescending(r => r.UpdatedDate ?? r.CreatedDate)
                .ToListAsync();

            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute(XNamespace.Xmlns + "xhtml", "http://www.w3.org/1999/xhtml"),
                    recipes.Select(recipe => new XElement("url",
                        new XElement("loc", $"{baseUrl}/tarifler"),
                        new XElement("lastmod", (recipe.UpdatedDate ?? recipe.CreatedDate).ToString("yyyy-MM-dd")),
                        new XElement("changefreq", "weekly"),
                        new XElement("priority", "0.8"),
                        GetHreflangAlternatives(baseUrl, "/tarifler")
                    ))
                )
            );

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Blog yazıları için sitemap
        /// </summary>
        [Route("sitemap-blog.xml")]
        [HttpGet]
        public async Task<IActionResult> SitemapBlog()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var blogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.UpdatedDate ?? b.CreatedDate)
                .ToListAsync();

            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute(XNamespace.Xmlns + "xhtml", "http://www.w3.org/1999/xhtml"),
                    blogs.Select(blog => new XElement("url",
                        new XElement("loc", $"{baseUrl}/Blog/Details/{blog.Id}"),
                        new XElement("lastmod", (blog.UpdatedDate ?? blog.CreatedDate).ToString("yyyy-MM-dd")),
                        new XElement("changefreq", "monthly"),
                        new XElement("priority", "0.7"),
                        GetHreflangAlternatives(baseUrl, $"/Blog/Details/{blog.Id}")
                    ))
                )
            );

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Hreflang alternatiflerini oluşturur (9 dil için)
        /// </summary>
        private XElement[] GetHreflangAlternatives(string baseUrl, string path)
        {
            var languages = new[] { "tr", "en", "de", "fr", "es", "ru", "ar", "ja", "ko" };
            
            return languages.Select(lang => 
                new XElement(XNamespace.Get("http://www.w3.org/1999/xhtml") + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", lang),
                    new XAttribute("href", $"{baseUrl}/{lang}{path}")
                )
            ).ToArray();
        }

        /// <summary>
        /// Donuk Gıda Nedir? - SEO Landing Page
        /// </summary>
        [Route("seo/donuk-gida-nedir")]
        [HttpGet]
        public IActionResult DonukGidaNedir()
        {
            ViewData["SeoTitle"] = "Donuk Gıda Nedir? | My Golden Food | Dondurulmuş Gıda Rehberi";
            ViewData["SeoDescription"] = "Donuk gıda nedir? Dondurulmuş gıda ile donuk gıda arasındaki farklar, flash freezing teknolojisi, donuk gıda üretim süreçleri ve saklama koşulları hakkında detaylı bilgi.";
            ViewData["SeoKeywords"] = "donuk gıda nedir, dondurulmuş gıda, flash freezing, donuk gıda üretimi, donuk gıda saklama, dondurulmuş gıda farkları, my golden food, ankara donuk gıda";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/seo/donuk-gida-nedir";
            ViewData["OgTitle"] = "Donuk Gıda Nedir? | My Golden Food - Dondurulmuş Gıda Rehberi";
            ViewData["OgDescription"] = "Donuk gıda nedir? Flash freezing teknolojisi ile üretilen donuk gıdalar hakkında detaylı bilgi.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1757011155/3lu-karisik-ormanmeyvesi_iwmlp2.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/seo/donuk-gida-nedir";

            // Breadcrumb
            ViewData["BreadcrumbItems"] = new System.Collections.Generic.List<MyGoldenFood.Models.BreadcrumbItem>
            {
                new MyGoldenFood.Models.BreadcrumbItem { Text = "Ana Sayfa", Url = Url.Action("Index", "Home") },
                new MyGoldenFood.Models.BreadcrumbItem { Text = "Donuk Gıda Nedir?", Url = Url.Action("DonukGidaNedir", "Seo"), IsActive = true }
            };

            return View();
        }
    }
}

