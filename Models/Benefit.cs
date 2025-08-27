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
    }
}
