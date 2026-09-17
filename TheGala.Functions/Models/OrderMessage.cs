namespace TheGala.Functions.Models
{
    // Mirrors TheGala.Models.OrderMessage in the web app. Kept as a separate copy
    // rather than a project reference so this Functions app stays an independently
    // deployable unit with no dependency on the ASP.NET Core MVC project.
    public class OrderMessage
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTimeOffset PlacedAtUtc { get; set; }
    }
}
