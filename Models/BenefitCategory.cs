using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGoldenFood.Models
{
    public class BenefitCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        
        // Tarih Alanları
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        // Alt kategori sistemi için yeni alanlar
        public int? ParentCategoryId { get; set; }
        public virtual BenefitCategory? ParentCategory { get; set; }
        public virtual ICollection<BenefitCategory> ChildCategories { get; set; } = new List<BenefitCategory>();
        public int Level { get; set; } = 0; // 0: Ana kategori, 1: Alt kategori
        public int SortOrder { get; set; } = 0; // Sıralama için
        
        // Mevcut ilişkiler
        public virtual ICollection<Benefit> Benefits { get; set; } = new List<Benefit>();
        public virtual ICollection<BenefitCategoryTranslation> Translations { get; set; } = new List<BenefitCategoryTranslation>();
        
        // Helper properties
        public bool IsParent => ChildCategories.Any();
        public bool IsChild => ParentCategoryId.HasValue;
        public string DisplayName => IsChild ? $"  {Name}" : Name; // Alt kategoriler için girinti
    }
}
