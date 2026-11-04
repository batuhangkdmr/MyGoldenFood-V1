using Microsoft.AspNetCore.Mvc;

namespace MyGoldenFood.Controllers
{
    public class TariflervefaydalariController : Controller
    {
        public IActionResult Index()
        {
            // SEO Bilgileri
            ViewData["SeoTitle"] = "Tarifler ve Faydaları | Dondurulmuş Gıda | My Golden Food";
            ViewData["SeoDescription"] = "Dondurulmuş gıda tarifleri ve sağlık faydaları. My Golden Food dondurulmuş meyve ve sebzeleri ile lezzetli tarifler ve beslenme bilgileri. Pratik yemek tarifleri ve sağlıklı beslenme rehberi.";
            ViewData["SeoKeywords"] = "dondurulmuş gıda tarifleri, donuk gıda faydaları, dondurulmuş meyve tarifleri, sağlıklı beslenme, pratik yemek tarifleri, my golden food";
            ViewData["CanonicalUrl"] = "https://mygoldenfood.com/tariflervefaydalari";
            ViewData["OgTitle"] = "Tarifler ve Faydaları | Dondurulmuş Gıda | My Golden Food";
            ViewData["OgDescription"] = "Dondurulmuş gıda tarifleri ve sağlık faydaları. Pratik yemek tarifleri ve sağlıklı beslenme rehberi.";
            ViewData["OgImage"] = "https://res.cloudinary.com/dbhogeepn/image/upload/v1743859767/mygolden_foood_a25y69.png";
            ViewData["OgUrl"] = "https://mygoldenfood.com/tariflervefaydalari";
            
            return View();
        }
    }
}
