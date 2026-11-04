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
            // SEO Bilgileri
            ViewData["SeoTitle"] = "Bayilik Başvuru Formu | My Golden Food | Bölge Bayiliği | Dondurulmuş Gıda Bayiliği";
            ViewData["SeoDescription"] = "My Golden Food bayilik başvuru formu. Bölge bayiliği fırsatları. Donuk depo işletmeleri için özel bayilik programı. Rekabetçi fiyatlandırma, profesyonel destek ve pazarlama yardımı. 24 saat içinde hızlı dönüş garantisi.";
            ViewData["SeoKeywords"] = "my golden food bayilik, dondurulmuş gıda bayiliği, bölge bayiliği, donuk gıda bayilik başvurusu, bayilik fırsatları, bayilik programı";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/Bayilik";
            ViewData["OgTitle"] = "Bayilik Başvuru Formu | My Golden Food | Bölge Bayiliği";
            ViewData["OgDescription"] = "My Golden Food bayilik başvuru formu. Donuk depo işletmeleri için özel bayilik programı.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1743859767/mygolden_foood_a25y69.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/Bayilik";
            
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
            string mesaj,
            bool kvkkConsent,
            bool marketingConsent)
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

                if (!kvkkConsent)
                {
                    return Json(new { success = false, message = "KVKK aydınlatma metnini onaylamanız gerekmektedir." });
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
                    mesaj,
                    kvkkConsent,
                    marketingConsent
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

