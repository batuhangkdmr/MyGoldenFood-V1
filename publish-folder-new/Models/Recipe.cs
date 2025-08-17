using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MyGoldenFood.Models
{
    public class Recipe : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int RecipeCategoryId { get; set; }
        public virtual RecipeCategory RecipeCategory { get; set; }
        public virtual ICollection<RecipeTranslation> RecipeTranslations { get; set; } = new List<RecipeTranslation>();
    }
}
