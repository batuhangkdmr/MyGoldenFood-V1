using System.Collections.Generic;

namespace MyGoldenFood.Models
{
    public class RecipeCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public virtual ICollection<RecipeCategoryTranslation> Translations { get; set; } = new List<RecipeCategoryTranslation>();
    }
}
