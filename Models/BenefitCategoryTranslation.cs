namespace MyGoldenFood.Models
{
    public class BenefitCategoryTranslation : BaseEntity
    {
        public int BenefitCategoryId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public virtual BenefitCategory BenefitCategory { get; set; } = null!;
    }
}
