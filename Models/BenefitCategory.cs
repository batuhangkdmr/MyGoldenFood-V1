using System.Collections.Generic;

namespace MyGoldenFood.Models
{
    public class BenefitCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<Benefit> Benefits { get; set; } = new List<Benefit>();
        public virtual ICollection<BenefitCategoryTranslation> Translations { get; set; } = new List<BenefitCategoryTranslation>();
    }
}
