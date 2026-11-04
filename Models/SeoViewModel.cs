namespace MyGoldenFood.Models
{
    public class SeoViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string CanonicalUrl { get; set; } = string.Empty;
        public string OgTitle { get; set; } = string.Empty;
        public string OgDescription { get; set; } = string.Empty;
        public string OgImage { get; set; } = string.Empty;
        public string OgUrl { get; set; } = string.Empty;
        public string OgType { get; set; } = "website";
        public string TwitterCard { get; set; } = "summary_large_image";
        public string TwitterTitle { get; set; } = string.Empty;
        public string TwitterDescription { get; set; } = string.Empty;
        public string TwitterImage { get; set; } = string.Empty;
        public string Robots { get; set; } = "index, follow";
        public bool NoIndex { get; set; } = false;
    }
}

