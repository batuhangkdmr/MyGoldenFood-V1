using System.Collections.Generic;
using System.Linq;

namespace MyGoldenFood.Models
{
    public class RecipeCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        
        // Alt kategori sistemi için yeni alanlar
        public int? ParentCategoryId { get; set; }
        public virtual RecipeCategory? ParentCategory { get; set; }
        public virtual ICollection<RecipeCategory> ChildCategories { get; set; } = new List<RecipeCategory>();
        public int Level { get; set; } = 0; // 0: Ana kategori, 1: Alt kategori
        public int SortOrder { get; set; } = 0; // Sıralama için
        
        // Mevcut ilişkiler
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public virtual ICollection<RecipeCategoryTranslation> Translations { get; set; } = new List<RecipeCategoryTranslation>();
        
        // Helper properties
        public bool IsParent => ChildCategories.Any();
        public bool IsChild => ParentCategoryId.HasValue;
        public string DisplayName => IsChild ? $"  {Name}" : Name; // Alt kategoriler için girinti
    }
}
