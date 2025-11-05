using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyGoldenFood.Services
{
    public class MailService
    {
        private readonly string smtpServer = "mail.mygoldenfood.com"; // SMTP Sunucusu
        private readonly int smtpPort = 465; // SMTP Port (SSL için)
        private readonly string smtpUsername = "info@mygoldenfood.com"; // SMTP Kullanıcı Adı
        private readonly string smtpPassword = "MYG1234myg"; // SMTP Şifresi

        public async Task<bool> SendEmailAsync(string senderName, string senderEmail, string messageContent)
        {
            try
            {
                // Null ve boş değer kontrolleri
                if (string.IsNullOrWhiteSpace(senderName) || 
                    string.IsNullOrWhiteSpace(senderEmail) || 
                    string.IsNullOrWhiteSpace(messageContent))
                {
                    System.Diagnostics.Debug.WriteLine("=== MailService: Zorunlu alanlar eksik ===");
                    return false;
                }

                // HTML encoding için helper method
                string HtmlEncode(string value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return WebUtility.HtmlEncode(value);
                }

                var emailMessage = new MimeMessage();
                
                // MailboxAddress oluştururken null kontrolü
                try
                {
                    emailMessage.From.Add(new MailboxAddress(HtmlEncode(senderName), senderEmail));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== MailService: From adresi hatası: {ex.Message} ===");
                    return false;
                }
                
                emailMessage.To.Add(new MailboxAddress("My Golden Food", smtpUsername));
                emailMessage.Subject = "Yeni İletişim Formu Mesajı";

                // Mesaj kısmı (HTML encoded)
                var mesajHtml = HtmlEncode(messageContent).Replace("\n", "<br>");

                // HTML e-posta içeriği
                var emailBody = "<p><strong>Yeni İletişim Formu Mesajı</strong></p>" +
                    "<table style=\"width: 100%; border-collapse: collapse; margin: 20px 0;\">" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; width: 40%; color: #555;\">Gönderen:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(senderName)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">E-posta:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(senderEmail)}</td></tr>" +
                    "</table>" +
                    "<div style=\"background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;\">" +
                    "<h3 style=\"margin-top: 0;\">Mesaj:</h3>" +
                    $"{mesajHtml}" +
                    "</div>" +
                    $"<p style=\"text-align: center; margin-top: 30px; color: #777; font-size: 12px;\">Bu e-posta My Golden Food web sitesinden otomatik olarak gönderilmiştir.<br>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>";

                emailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailBody
                };

                System.Diagnostics.Debug.WriteLine("=== MailService: SMTP bağlantısı başlatılıyor ===");
                System.Diagnostics.Debug.WriteLine($"SMTP Server: {smtpServer}");
                System.Diagnostics.Debug.WriteLine($"SMTP Port: {smtpPort}");
                System.Diagnostics.Debug.WriteLine($"SMTP Username: {smtpUsername}");
                
                using var smtp = new SmtpClient();
                
                // SSL sertifika doğrulamasını bypass et (geliştirme/production için)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("ConnectAsync çağrılıyor...");
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);
                    System.Diagnostics.Debug.WriteLine("=== MailService: SMTP bağlantısı başarılı ===");
                }
                catch (Exception connectEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP CONNECTION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {connectEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {connectEx.StackTrace}");
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticateAsync çağrılıyor...");
                    await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                    System.Diagnostics.Debug.WriteLine("=== MailService: SMTP authentication başarılı ===");
                }
                catch (Exception authEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP AUTHENTICATION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {authEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {authEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendAsync çağrılıyor...");
                    await smtp.SendAsync(emailMessage);
                    System.Diagnostics.Debug.WriteLine("=== MailService: Email gönderildi ===");
                }
                catch (Exception sendEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP SEND HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {sendEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {sendEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                await smtp.DisconnectAsync(true);
                System.Diagnostics.Debug.WriteLine("=== MailService: BAŞARILI ===");

                return true;
            }
            catch (Exception ex)
            {
                // Exception detaylarını hem Debug hem Console'a yazdır
                var errorDetails = $"=== MailService SendEmailAsync EXCEPTION ===\n" +
                    $"Exception Type: {ex.GetType().FullName}\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n";
                
                if (ex.InnerException != null)
                {
                    errorDetails += $"InnerException Type: {ex.InnerException.GetType().FullName}\n" +
                        $"InnerException Message: {ex.InnerException.Message}\n" +
                        $"InnerException StackTrace: {ex.InnerException.StackTrace}\n";
                }
                
                errorDetails += "============================================================\n";
                
                // Debug Output
                System.Diagnostics.Debug.WriteLine(errorDetails);
                
                // Console Output (daha görünür)
                Console.WriteLine(errorDetails);
                
                // Exception'ın tüm detaylarını yazdır
                System.Diagnostics.Debug.WriteLine($"Exception ToString: {ex.ToString()}");
                Console.WriteLine($"Exception ToString: {ex.ToString()}");
                
                return false;
            }
        }

        public async Task<bool> SendDealershipApplicationAsync(
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
                // Null ve boş değer kontrolleri
                if (string.IsNullOrWhiteSpace(firmaAdi) || 
                    string.IsNullOrWhiteSpace(yetkiliKisi) || 
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(sehir) ||
                    string.IsNullOrWhiteSpace(depoDurumu))
                {
                    System.Diagnostics.Debug.WriteLine("=== MailService: Zorunlu alanlar eksik ===");
                    return false;
                }

                if (secilenUrunler == null || secilenUrunler.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("=== MailService: Ürün seçimi yapılmamış ===");
                    return false;
                }

                // HTML encoding için helper method
                string HtmlEncode(string value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return WebUtility.HtmlEncode(value);
                }

                var emailMessage = new MimeMessage();
                
                // MailboxAddress oluştururken null kontrolü
                try
                {
                    emailMessage.From.Add(new MailboxAddress(HtmlEncode(yetkiliKisi), email));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== MailService: From adresi hatası: {ex.Message} ===");
                    return false;
                }
                
                emailMessage.To.Add(new MailboxAddress("My Golden Food", smtpUsername));
                emailMessage.Subject = $"Yeni Bölge Bayilik Başvurusu - {HtmlEncode(firmaAdi)}";

                // Ürün listesi HTML formatında (null-safe)
                var urunListesi = string.Join("<br>", secilenUrunler
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => "• " + HtmlEncode(u)));

                if (string.IsNullOrEmpty(urunListesi))
                {
                    urunListesi = "Ürün seçimi yapılmamış";
                }

                // Mesaj kısmı (HTML encoded)
                var mesajHtml = string.IsNullOrWhiteSpace(mesaj) 
                    ? "" 
                    : $"<div style=\"background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 20px 0;\"><strong>Mesaj/Not:</strong><br>{HtmlEncode(mesaj).Replace("\n", "<br>")}</div>";

                // HTML e-posta içeriği - Tüm kullanıcı girdileri HTML encoded
                var emailBody = "<p><strong>Yeni Bölge Bayilik Başvurusu</strong></p>" +
                    "<table style=\"width: 100%; border-collapse: collapse; margin: 20px 0;\">" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; width: 40%; color: #555;\">Firma Adı:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(firmaAdi)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">Yetkili Kişi:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(yetkiliKisi)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">E-posta:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(email)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">Telefon:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(telefon ?? "Belirtilmemiş")}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">Şehir/Bölge:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(sehir)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">Donuk Depo Durumu:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\"><strong>{HtmlEncode(depoDurumu)}</strong></td></tr>" +
                    "</table>" +
                    "<div style=\"background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;\">" +
                    "<h3 style=\"margin-top: 0;\">Seçilen Ürünler:</h3>" +
                    $"{urunListesi}" +
                    "</div>" +
                    mesajHtml +
                    $"<p style=\"text-align: center; margin-top: 30px; color: #777; font-size: 12px;\">Bu e-posta My Golden Food web sitesinden otomatik olarak gönderilmiştir.<br>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>";

                emailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailBody
                };

                System.Diagnostics.Debug.WriteLine("=== MailService: SMTP bağlantısı başlatılıyor ===");
                System.Diagnostics.Debug.WriteLine($"SMTP Server: {smtpServer}");
                System.Diagnostics.Debug.WriteLine($"SMTP Port: {smtpPort}");
                System.Diagnostics.Debug.WriteLine($"SMTP Username: {smtpUsername}");
                
                using var smtp = new SmtpClient();
                
                // SSL sertifika doğrulamasını bypass et (geliştirme/production için)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("ConnectAsync çağrılıyor...");
                    // SecureSocketOptions.Auto kullan - MailKit otomatik olarak en iyi seçeneği seçer
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);
                    System.Diagnostics.Debug.WriteLine("=== MailService: SMTP bağlantısı başarılı ===");
                }
                catch (Exception connectEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP CONNECTION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {connectEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {connectEx.StackTrace}");
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticateAsync çağrılıyor...");
                    await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                    System.Diagnostics.Debug.WriteLine("=== MailService: SMTP authentication başarılı ===");
                }
                catch (Exception authEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP AUTHENTICATION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {authEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {authEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendAsync çağrılıyor...");
                    await smtp.SendAsync(emailMessage);
                    System.Diagnostics.Debug.WriteLine("=== MailService: Email gönderildi ===");
                }
                catch (Exception sendEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SMTP SEND HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {sendEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {sendEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                await smtp.DisconnectAsync(true);
                System.Diagnostics.Debug.WriteLine("=== MailService: BAŞARILI ===");

                return true;
            }
            catch (Exception ex)
            {
                // Exception detaylarını hem Debug hem Console'a yazdır
                var errorDetails = $"=== MailService SendDealershipApplicationAsync EXCEPTION ===\n" +
                    $"Exception Type: {ex.GetType().FullName}\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n";
                
                if (ex.InnerException != null)
                {
                    errorDetails += $"InnerException Type: {ex.InnerException.GetType().FullName}\n" +
                        $"InnerException Message: {ex.InnerException.Message}\n" +
                        $"InnerException StackTrace: {ex.InnerException.StackTrace}\n";
                }
                
                errorDetails += "============================================================\n";
                
                // Debug Output
                System.Diagnostics.Debug.WriteLine(errorDetails);
                
                // Console Output (daha görünür)
                Console.WriteLine(errorDetails);
                
                // Exception'ın tüm detaylarını yazdır
                System.Diagnostics.Debug.WriteLine($"Exception ToString: {ex.ToString()}");
                Console.WriteLine($"Exception ToString: {ex.ToString()}");
                
                return false;
            }
        }

        public async Task<bool> SendContactMessageAsync(string senderName, string senderEmail, string messageContent)
        {
            try
            {
                // Null ve boş değer kontrolleri
                if (string.IsNullOrWhiteSpace(senderName) || 
                    string.IsNullOrWhiteSpace(senderEmail) || 
                    string.IsNullOrWhiteSpace(messageContent))
                {
                    System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: Zorunlu alanlar eksik ===");
                    return false;
                }

                // HTML encoding için helper method
                string HtmlEncode(string value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return WebUtility.HtmlEncode(value);
                }

                var emailMessage = new MimeMessage();
                
                // MailboxAddress oluştururken null kontrolü
                try
                {
                    emailMessage.From.Add(new MailboxAddress(HtmlEncode(senderName), senderEmail));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== MailService SendContactMessageAsync: From adresi hatası: {ex.Message} ===");
                    return false;
                }
                
                emailMessage.To.Add(new MailboxAddress("My Golden Food", smtpUsername));
                emailMessage.Subject = $"Yeni İletişim Formu Mesajı - {HtmlEncode(senderName)}";

                // Mesaj kısmı (HTML encoded)
                var mesajHtml = HtmlEncode(messageContent).Replace("\n", "<br>");

                // HTML e-posta içeriği - Tüm kullanıcı girdileri HTML encoded
                var emailBody = "<p><strong>Yeni İletişim Formu Mesajı</strong></p>" +
                    "<table style=\"width: 100%; border-collapse: collapse; margin: 20px 0;\">" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; width: 40%; color: #555;\">Gönderen:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(senderName)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">E-posta:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(senderEmail)}</td></tr>" +
                    "</table>" +
                    "<div style=\"background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;\">" +
                    "<h3 style=\"margin-top: 0;\">Mesaj:</h3>" +
                    $"{mesajHtml}" +
                    "</div>" +
                    $"<p style=\"text-align: center; margin-top: 30px; color: #777; font-size: 12px;\">Bu e-posta My Golden Food web sitesinden otomatik olarak gönderilmiştir.<br>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>";

                emailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailBody
                };

                System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: SMTP bağlantısı başlatılıyor ===");
                System.Diagnostics.Debug.WriteLine($"SMTP Server: {smtpServer}");
                System.Diagnostics.Debug.WriteLine($"SMTP Port: {smtpPort}");
                System.Diagnostics.Debug.WriteLine($"SMTP Username: {smtpUsername}");

                using var smtp = new SmtpClient();
                
                // SSL sertifika doğrulamasını bypass et (geliştirme/production için)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendContactMessageAsync: ConnectAsync çağrılıyor...");
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: SMTP bağlantısı başarılı ===");
                }
                catch (Exception connectEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendContactMessageAsync SMTP CONNECTION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {connectEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {connectEx.StackTrace}");
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendContactMessageAsync: AuthenticateAsync çağrılıyor...");
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: SMTP authentication başarılı ===");
                }
                catch (Exception authEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendContactMessageAsync SMTP AUTHENTICATION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {authEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {authEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendContactMessageAsync: SendAsync çağrılıyor...");
                    await smtp.SendAsync(emailMessage);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: Email gönderildi ===");
                }
                catch (Exception sendEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendContactMessageAsync SMTP SEND HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {sendEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {sendEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                await smtp.DisconnectAsync(true);
                System.Diagnostics.Debug.WriteLine("=== MailService SendContactMessageAsync: BAŞARILI ===");

                return true;
            }
            catch (Exception ex)
            {
                // Exception detaylarını hem Debug hem Console'a yazdır
                var errorDetails = $"=== MailService SendContactMessageAsync EXCEPTION ===\n" +
                    $"Exception Type: {ex.GetType().FullName}\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n";
                
                if (ex.InnerException != null)
                {
                    errorDetails += $"InnerException Type: {ex.InnerException.GetType().FullName}\n" +
                        $"InnerException Message: {ex.InnerException.Message}\n" +
                        $"InnerException StackTrace: {ex.InnerException.StackTrace}\n";
                }
                
                errorDetails += "============================================================\n";
                
                // Debug Output
                System.Diagnostics.Debug.WriteLine(errorDetails);
                
                // Console Output (daha görünür)
                Console.WriteLine(errorDetails);
                
                // Exception'ın tüm detaylarını yazdır
                System.Diagnostics.Debug.WriteLine($"Exception ToString: {ex.ToString()}");
                Console.WriteLine($"Exception ToString: {ex.ToString()}");
                
                return false;
            }
        }

        public async Task<bool> SendIletisimFormMessageAsync(string adsoyad, string email, string konu, string mesaj)
        {
            try
            {
                // Null ve boş değer kontrolleri
                if (string.IsNullOrWhiteSpace(adsoyad) || 
                    string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(konu) ||
                    string.IsNullOrWhiteSpace(mesaj))
                {
                    System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: Zorunlu alanlar eksik ===");
                    return false;
                }

                // HTML encoding için helper method
                string HtmlEncode(string value)
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    return WebUtility.HtmlEncode(value);
                }

                var emailMessage = new MimeMessage();
                
                // MailboxAddress oluştururken null kontrolü
                try
                {
                    emailMessage.From.Add(new MailboxAddress(HtmlEncode(adsoyad), email));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== MailService SendIletisimFormMessageAsync: From adresi hatası: {ex.Message} ===");
                    return false;
                }
                
                emailMessage.To.Add(new MailboxAddress("My Golden Food", smtpUsername));
                emailMessage.Subject = HtmlEncode(konu);

                // Mesaj kısmı (HTML encoded)
                var mesajHtml = HtmlEncode(mesaj).Replace("\n", "<br>");

                // HTML e-posta içeriği - Tüm kullanıcı girdileri HTML encoded
                var emailBody = "<p><strong>Yeni İletişim Formu Mesajı</strong></p>" +
                    "<table style=\"width: 100%; border-collapse: collapse; margin: 20px 0;\">" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; width: 40%; color: #555;\">Gönderen:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(adsoyad)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">E-posta:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\">{HtmlEncode(email)}</td></tr>" +
                    "<tr><td style=\"padding: 10px; border-bottom: 1px solid #eee; font-weight: bold; color: #555;\">Konu:</td>" +
                    $"<td style=\"padding: 10px; border-bottom: 1px solid #eee;\"><strong>{HtmlEncode(konu)}</strong></td></tr>" +
                    "</table>" +
                    "<div style=\"background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;\">" +
                    "<h3 style=\"margin-top: 0;\">Mesaj:</h3>" +
                    $"{mesajHtml}" +
                    "</div>" +
                    $"<p style=\"text-align: center; margin-top: 30px; color: #777; font-size: 12px;\">Bu e-posta My Golden Food web sitesinden otomatik olarak gönderilmiştir.<br>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>";

                emailMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = emailBody
                };

                System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: SMTP bağlantısı başlatılıyor ===");
                System.Diagnostics.Debug.WriteLine($"SMTP Server: {smtpServer}");
                System.Diagnostics.Debug.WriteLine($"SMTP Port: {smtpPort}");
                System.Diagnostics.Debug.WriteLine($"SMTP Username: {smtpUsername}");
                
                using var smtp = new SmtpClient();
                
                // SSL sertifika doğrulamasını bypass et (geliştirme/production için)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendIletisimFormMessageAsync: ConnectAsync çağrılıyor...");
                    await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: SMTP bağlantısı başarılı ===");
                }
                catch (Exception connectEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendIletisimFormMessageAsync SMTP CONNECTION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {connectEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {connectEx.StackTrace}");
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendIletisimFormMessageAsync: AuthenticateAsync çağrılıyor...");
                    await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: SMTP authentication başarılı ===");
                }
                catch (Exception authEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendIletisimFormMessageAsync SMTP AUTHENTICATION HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {authEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {authEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("SendIletisimFormMessageAsync: SendAsync çağrılıyor...");
                    await smtp.SendAsync(emailMessage);
                    System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: Email gönderildi ===");
                }
                catch (Exception sendEx)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SendIletisimFormMessageAsync SMTP SEND HATASI ===");
                    System.Diagnostics.Debug.WriteLine($"Hata: {sendEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack: {sendEx.StackTrace}");
                    await smtp.DisconnectAsync(true);
                    throw;
                }
                
                await smtp.DisconnectAsync(true);
                System.Diagnostics.Debug.WriteLine("=== MailService SendIletisimFormMessageAsync: BAŞARILI ===");

                return true;
            }
            catch (Exception ex)
            {
                // Exception detaylarını hem Debug hem Console'a yazdır
                var errorDetails = $"=== MailService SendIletisimFormMessageAsync EXCEPTION ===\n" +
                    $"Exception Type: {ex.GetType().FullName}\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n";
                
                if (ex.InnerException != null)
                {
                    errorDetails += $"InnerException Type: {ex.InnerException.GetType().FullName}\n" +
                        $"InnerException Message: {ex.InnerException.Message}\n" +
                        $"InnerException StackTrace: {ex.InnerException.StackTrace}\n";
                }
                
                errorDetails += "============================================================\n";
                
                // Debug Output
                System.Diagnostics.Debug.WriteLine(errorDetails);
                
                // Console Output (daha görünür)
                Console.WriteLine(errorDetails);
                
                // Exception'ın tüm detaylarını yazdır
                System.Diagnostics.Debug.WriteLine($"Exception ToString: {ex.ToString()}");
                Console.WriteLine($"Exception ToString: {ex.ToString()}");
                
                return false;
            }
        }
    }
}
