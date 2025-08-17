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

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IConfiguration configuration, IHubContext<ProductHub> hubContext)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;

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
            return View();
        }

        public IActionResult About()
        {
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

            return View(products);
        }

        [HttpGet]
        public IActionResult Iletisim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Iletisim(string adsoyad, string email, string konu, string mesaj)
        {
            try
            {
                // Türkiye saatini al
                var turkiyeSaati = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("My Golden Food", _smtpUsername));

                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail1));
                emailMessage.To.Add(new MailboxAddress("Admin", _recipientEmail2));

                emailMessage.Subject = konu;
                emailMessage.Date = turkiyeSaati;

                emailMessage.Body = new TextPart("html")
                {
                    Text = $"<strong>Gönderen:</strong> {adsoyad} ({email}) <br><br> " +
                           $"<strong>Mesaj:</strong> {mesaj}"
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.Auto);
                    client.Authenticate(_smtpUsername, _smtpPassword);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                ViewBag.Uyari = "Mesajınız başarıyla gönderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Uyari = "Mesaj gönderilirken hata oluştu: " + ex.Message;
            }

            return View();
        }
    }
}
