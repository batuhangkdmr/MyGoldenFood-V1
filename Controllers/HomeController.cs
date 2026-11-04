using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using System;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MyGoldenFood.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MyGoldenFood.Services;

namespace MyGoldenFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _recipientEmail1;
        private readonly string _recipientEmail2;
        private readonly IHubContext<ProductHub> _hubContext;
        private readonly MyGoldenFood.Services.CloudinaryService _cloudinaryService;
        private readonly MailService _mailService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IConfiguration configuration, IHubContext<ProductHub> hubContext, MyGoldenFood.Services.CloudinaryService cloudinaryService, MailService mailService)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
            _cloudinaryService = cloudinaryService;
            _mailService = mailService;

            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpServer = emailSettings["SmtpServer"];
            _smtpPort = int.Parse(emailSettings["Port"]);
            _smtpUsername = emailSettings["Username"];
            _smtpPassword = emailSettings["Password"];
            _recipientEmail1 = emailSettings["RecipientEmail1"];
            _recipientEmail2 = emailSettings["RecipientEmail2"];
        }

        public IActionResult Index()
        {
            // SEO Bilgileri
            ViewData["SeoTitle"] = "My Golden Food | Premium Dondurulmuş Gıda | Ankara | Flash Freezing Teknolojisi";
            ViewData["SeoDescription"] = "My Golden Food - Premium dondurulmuş gıda çözümleri sunuyoruz. Kaliteli donuk gıda ürünleri, dondurulmuş meyve ve sebzeler. Flash freezing teknolojisi ile tazelik garantisi. Ankara'da güvenilir dondurulmuş gıda tedarikçisi.";
            ViewData["SeoKeywords"] = "donuk gıda, dondurulmuş gıda, dondurulmuş meyve, dondurulmuş sebze, my golden food, ankara donuk gıda, türkiye dondurulmuş gıda, flash freezing, dondurulmuş çilek, dondurulmuş böğürtlen, dondurulmuş mantar, dondurulmuş brokoli, premium donuk gıda";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com";
            ViewData["OgTitle"] = "My Golden Food - Premium Dondurulmuş Gıda Çözümleri";
            ViewData["OgDescription"] = "Premium dondurulmuş gıda çözümleri sunuyoruz. Kaliteli donuk gıda ürünleri, flash freezing teknolojisi ile tazelik garantisi.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1757011155/3lu-karisik-ormanmeyvesi_iwmlp2.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com";
            ViewData["OgType"] = "website";
            
            return View();
        }

        public IActionResult About()
        {
            // SEO Bilgileri
            ViewData["SeoTitle"] = "Hakkımızda | My Golden Food | 15 Yıllık Deneyim | Flash Freezing Teknolojisi";
            ViewData["SeoDescription"] = "My Golden Food hakkında bilgiler. 15 yıllık deneyim, 50.000+ mutlu müşteri, 100+ ürün çeşidi ile dondurulmuş gıda sektöründe öncü marka. Flash freezing teknolojisi ile kaliteli üretim.";
            ViewData["SeoKeywords"] = "my golden food hakkında, dondurulmuş gıda şirketi, ankara donuk gıda üreticisi, flash freezing teknolojisi, 15 yıllık deneyim, kaliteli dondurulmuş gıda";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/hakkimizda";
            ViewData["OgTitle"] = "Hakkımızda | My Golden Food | 15 Yıllık Deneyim";
            ViewData["OgDescription"] = "15 yıllık deneyim, 50.000+ mutlu müşteri, 100+ ürün çeşidi ile dondurulmuş gıda sektöründe öncü marka.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1743859767/mygolden_foood_a25y69.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/hakkimizda";
            
            return View();
        }

        // Ürünler Sayfası
        public async Task<IActionResult> Products()
        {
            var userCulture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            string selectedLanguage = "tr";

            if (!string.IsNullOrEmpty(userCulture))
            {
                selectedLanguage = userCulture.Split('|')[0].Replace("c=", "");
            }

            var products = await _context.Products.ToListAsync();

            // Eğer Türkçe değilse Translation tablosundan çek
            if (selectedLanguage != "tr")
            {
                foreach (var product in products)
                {
                    var translatedName = await _context.Translations
                        .Where(t => t.ReferenceId == product.Id && t.TableName == "Product" && t.FieldName == "Name" && t.Language == selectedLanguage)
                        .Select(t => t.TranslatedValue)
                        .FirstOrDefaultAsync();

                    var translatedDescription = await _context.Translations
                        .Where(t => t.ReferenceId == product.Id && t.TableName == "Product" && t.FieldName == "Description" && t.Language == selectedLanguage)
                        .Select(t => t.TranslatedValue)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(translatedName))
                        product.Name = translatedName;

                    if (!string.IsNullOrEmpty(translatedDescription))
                        product.Description = translatedDescription;
                }
            }

            ViewBag.CloudinaryService = _cloudinaryService; // 🚀 Responsive resimler için
            
            // SEO Bilgileri
            ViewData["SeoTitle"] = "Dondurulmuş Gıda Ürünlerimiz | Donuk Gıda Kataloğu | My Golden Food | 350g Paketler";
            ViewData["SeoDescription"] = "My Golden Food dondurulmuş gıda ürün kataloğu. Dondurulmuş meyve, sebze ve hazır gıdalar. 350 gram paketlerde taze donuk gıda ürünleri. Flash freezing teknolojisi ile korunan besin değeri. Ankara'da kaliteli dondurulmuş gıda tedarikçisi.";
            ViewData["SeoKeywords"] = "dondurulmuş gıda ürünleri, donuk gıda kataloğu, dondurulmuş meyve, dondurulmuş sebze, my golden food ürünleri, 350 gram paket, flash freezing, ankara dondurulmuş gıda, türkiye donuk gıda tedarikçisi, dondurulmuş çilek, dondurulmuş böğürtlen";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/urunler";
            ViewData["OgTitle"] = "Dondurulmuş Gıda Ürünlerimiz | Donuk Gıda Kataloğu";
            ViewData["OgDescription"] = "My Golden Food dondurulmuş gıda ürün kataloğu. 350 gram paketlerde taze ve kaliteli donuk gıda ürünleri.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1743859767/mygolden_foood_a25y69.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/urunler";
            
            return View(products);
        }

        [HttpGet]
        public IActionResult Iletisim()
        {
            // SEO Bilgileri
            ViewData["SeoTitle"] = "İletişim | My Golden Food | Ankara Dondurulmuş Gıda | +90 533 635 24 58";
            ViewData["SeoDescription"] = "My Golden Food ile iletişime geçin. Ankara'da dondurulmuş gıda tedarikçisi. Telefon: +90 533 635 24 58, E-posta: info@mygoldenfood.com. Bayilik başvuruları ve ürün bilgileri için bizimle iletişime geçin.";
            ViewData["SeoKeywords"] = "my golden food iletişim, ankara dondurulmuş gıda iletişim, donuk gıda telefon, my golden food telefon, dondurulmuş gıda bayilik başvurusu";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/iletisim";
            ViewData["OgTitle"] = "İletişim | My Golden Food | Ankara Dondurulmuş Gıda";
            ViewData["OgDescription"] = "My Golden Food ile iletişime geçin. Telefon: +90 533 635 24 58, E-posta: info@mygoldenfood.com";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1743859767/mygolden_foood_a25y69.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/iletisim";
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Iletisim(string adsoyad, string email, string konu, string mesaj)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== HomeController Iletisim POST DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"adsoyad: {adsoyad}");
                System.Diagnostics.Debug.WriteLine($"email: {email}");
                System.Diagnostics.Debug.WriteLine($"konu: {konu}");
                System.Diagnostics.Debug.WriteLine($"mesaj length: {mesaj?.Length ?? 0}");
                Console.WriteLine("=== HomeController Iletisim POST DEBUG ===");
                Console.WriteLine($"adsoyad: {adsoyad}");
                Console.WriteLine($"email: {email}");
                Console.WriteLine($"konu: {konu}");

                // Validation
                if (string.IsNullOrWhiteSpace(adsoyad) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(konu) ||
                    string.IsNullOrWhiteSpace(mesaj))
                {
                    System.Diagnostics.Debug.WriteLine("=== HomeController Iletisim: Validation failed ===");
                    ViewBag.Uyari = "Lütfen tüm alanları doldurunuz.";
                    return View();
                }

                // Mail gönder
                System.Diagnostics.Debug.WriteLine("=== HomeController: Calling MailService.SendIletisimFormMessageAsync ===");
                var result = await _mailService.SendIletisimFormMessageAsync(adsoyad, email, konu, mesaj);
                System.Diagnostics.Debug.WriteLine($"=== HomeController: MailService result: {result} ===");
                Console.WriteLine($"=== HomeController: MailService result: {result} ===");

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("=== HomeController Iletisim: SUCCESS ===");
                    ViewBag.Uyari = "Mesajınız başarıyla gönderildi! En kısa sürede (24 saat içinde) sizlere dönüş sağlayacağız.";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("=== HomeController Iletisim: MailService returned false ===");
                    ViewBag.Uyari = "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyiniz.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("=== HomeController Iletisim EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("=== HomeController Iletisim EXCEPTION ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                ViewBag.Uyari = "Mesaj gönderilirken hata oluştu: " + ex.Message;
            }

            return View();
        }
    }
}
