using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MyGoldenFood.Models
{
    public class Benefit : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int? BenefitCategoryId { get; set; }
        public virtual BenefitCategory? BenefitCategory { get; set; }
        public virtual ICollection<BenefitTranslation> Translations { get; set; } = new List<BenefitTranslation>();
        
        // Tarih Alanları
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        // SEO Alanları
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? SeoSlug { get; set; }
        public string? PreparationTime { get; set; } // Hazırlık süresi
        public string? CookingTime { get; set; } // Pişirme süresi
        public int? Servings { get; set; } // Porsiyon sayısı
        public string? Difficulty { get; set; } // Zorluk seviyesi
    }
}
