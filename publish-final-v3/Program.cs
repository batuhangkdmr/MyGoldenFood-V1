using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Services;
using System.Net;
using System.Net.Mail;
using MyGoldenFood.Hubs;
using Microsoft.Extensions.Localization;

namespace MyGoldenFood
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 🌐 Cloudinary Servisi
            services.AddSingleton(sp =>
            {
                var cloudinaryConfig = Configuration.GetSection("CloudinarySettings");
                var account = new Account(
                    cloudinaryConfig["CloudName"],
                    cloudinaryConfig["ApiKey"],
                    cloudinaryConfig["ApiSecret"]
                );
                return new Cloudinary(account);
            });

            // 💼 DI Container Kayıtları
            services.AddScoped<CloudinaryService>();
            services.AddScoped<DeepLTranslationService>();
            services.AddScoped<MailService>();
            services.AddScoped<LocalizationCacheService>();
            services.AddScoped<TranslationService>();
            
            // 🌐 JSON Translation Service
            services.AddScoped<IJsonTranslationService, JsonTranslationService>();

            // 💾 DB Ayarı
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // 🔐 Authentication
            services.AddAuthentication("AdminCookie")
                .AddCookie("AdminCookie", options =>
                {
                    options.LoginPath = "/Admin/Index";
                    options.AccessDeniedPath = "/Admin/Index";
                });

            // 📧 Mail Ayarları
            var emailSettings = Configuration.GetSection("EmailSettings");
            services.AddScoped<SmtpClient>(sp =>
            {
                return new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]))
                {
                    Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                    EnableSsl = true
                };
            });

            // 🧠 Memory Cache
            services.AddMemoryCache();

            // 🟢 Session
            services.AddSession();

            // 📡 SignalR
            services.AddSignalR();

            // MVC
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ⚠️ Hata Ayarları
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            
            // 🌐 Session support for language switching
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            // 📡 SignalR Hub'ları
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ProductHub>("/productHub");
                endpoints.MapHub<FaydalariHub>("/faydalariHub");
                endpoints.MapHub<TariflerHub>("/tariflerHub");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
