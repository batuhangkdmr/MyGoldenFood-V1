using Microsoft.AspNetCore.Mvc;
using MyGoldenFood.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyGoldenFood.Controllers
{
    public class DealershipController : Controller
    {
        private readonly MailService _mailService;

        public DealershipController(MailService mailService)
        {
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Bayilik/Submit")]
        public async Task<IActionResult> Submit(
            string firmaAdi,
            string yetkiliKisi,
            string email,
            string telefon,
            string sehir,
            string depoDurumu,
            string[] secilenUrunler,
            string mesaj)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(firmaAdi) ||
                    string.IsNullOrWhiteSpace(yetkiliKisi) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(sehir) ||
                    string.IsNullOrWhiteSpace(depoDurumu))
                {
                    return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurunuz." });
                }

                // Ürün seçimi kontrolü
                if (secilenUrunler == null || secilenUrunler.Length == 0)
                {
                    return Json(new { success = false, message = "Lütfen en az bir ürün seçiniz." });
                }

                // Mail gönder
                var result = await _mailService.SendDealershipApplicationAsync(
                    firmaAdi,
                    yetkiliKisi,
                    email,
                    telefon,
                    sehir,
                    depoDurumu,
                    secilenUrunler,
                    mesaj
                );

                if (result)
                {
                    return Json(new { 
                        success = true, 
                        message = "Başvurunuz başarıyla gönderildi! En kısa sürede (24 saat içinde) sizlere dönüş sağlayacağız." 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Başvuru gönderilirken bir hata oluştu. Lütfen tekrar deneyiniz." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
}

