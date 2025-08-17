using System.ComponentModel.DataAnnotations.Schema;

namespace MyGoldenFood.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Description { get; set; } = string.Empty;

        // Fotoğraf için dosya yolu
        public string ImagePath { get; set; } = string.Empty;
    }
}
