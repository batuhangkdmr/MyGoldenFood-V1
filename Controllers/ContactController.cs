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

      
    }
}