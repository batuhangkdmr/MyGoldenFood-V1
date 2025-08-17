using Microsoft.AspNetCore.Mvc;
using MyGoldenFood.Services;
using System;
using System.Threading.Tasks;

namespace MyGoldenFood.Controllers
{
    public class ContactController : Controller
    {
        private readonly MailService _mailService;

        public ContactController(MailService mailService)
        {
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string adsoyad, string email, string konu, string mesaj)
        {
            try
            {
                var result = await _mailService.SendEmailAsync(adsoyad, email, mesaj);
                if (result)
                {
                    ViewBag.Uyari = "Mesajınız başarıyla gönderildi!";
                }
                else
                {
                    ViewBag.Uyari = "Mesaj gönderilirken hata oluştu!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Uyari = "Mesaj gönderilirken hata oluştu: " + ex.Message;
            }

            return View();
        }
    }
}