using System;
using System.ComponentModel.DataAnnotations;

namespace MyGoldenFood.Models
{
    public class Blog : BaseEntity
    {
        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik alanı zorunludur")]
        public string Content { get; set; }

        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olabilir")]
        public string Summary { get; set; }

        [StringLength(200, ErrorMessage = "Resim yolu en fazla 200 karakter olabilir")]
        public string ImagePath { get; set; }

        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir")]
        public string Category { get; set; }

        [StringLength(100, ErrorMessage = "Etiketler en fazla 100 karakter olabilir")]
        public string Tags { get; set; }

        [Required]
        public string Author { get; set; } = "Admin";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public bool IsPublished { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        [StringLength(200, ErrorMessage = "SEO başlığı en fazla 200 karakter olabilir")]
        public string SeoTitle { get; set; }

        [StringLength(500, ErrorMessage = "SEO açıklaması en fazla 500 karakter olabilir")]
        public string SeoDescription { get; set; }

        [StringLength(200, ErrorMessage = "SEO anahtar kelimeleri en fazla 200 karakter olabilir")]
        public string SeoKeywords { get; set; }

        [StringLength(200, ErrorMessage = "SEO URL en fazla 200 karakter olabilir")]
        public string SeoUrl { get; set; }
    }
}
