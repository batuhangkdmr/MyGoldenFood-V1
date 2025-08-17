using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyGoldenFood.Services
{
    public static class ViewTranslationHelper
    {
        public static string T(this IHtmlHelper htmlHelper, string key, string language = null)
        {
            var translationService = htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(IJsonTranslationService)) as IJsonTranslationService;
            if (translationService == null)
            {
                return key;
            }

            if (string.IsNullOrEmpty(language))
            {
                language = GetCurrentLanguage(htmlHelper.ViewContext.HttpContext);
            }

            return translationService.GetTranslation(key, language);
        }

        public static string T(this IUrlHelper urlHelper, string key, string language = null)
        {
            var translationService = urlHelper.ActionContext.HttpContext.RequestServices.GetService(typeof(IJsonTranslationService)) as IJsonTranslationService;
            if (translationService == null)
            {
                return key;
            }

            if (string.IsNullOrEmpty(language))
            {
                language = GetCurrentLanguage(urlHelper.ActionContext.HttpContext);
            }

            return translationService.GetTranslation(key, language);
        }

        private static string GetCurrentLanguage(HttpContext httpContext)
        {
            // 1. Session (varsa)
            var sessionLanguage = httpContext.Session.GetString("Language");
            if (!string.IsNullOrEmpty(sessionLanguage))
                return sessionLanguage;

            // 2. ASP.NET Core culture cookie
            var cultureCookie = httpContext.Request.Cookies[Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrEmpty(cultureCookie))
            {
                // .AspNetCore.Culture=c=tr|uic=tr gibi bir değer olur, ilk "c=" sonrası iki harfi al
                var cIndex = cultureCookie.IndexOf("c=");
                if (cIndex >= 0)
                {
                    var lang = cultureCookie.Substring(cIndex + 2, 2);
                    return lang;
                }
            }

            // 3. Default
            return "tr";
        }
    }
} 