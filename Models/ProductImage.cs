namespace TheGala.Models
{
    // Simple view model used to display an uploaded image in the gallery.
    // Name is the blob's name in the "product-images" container;
    // Url points at our own controller action that streams the blob back.
    public class ProductImage
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
