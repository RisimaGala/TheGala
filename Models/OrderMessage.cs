namespace TheGala.Models
{
    // The JSON payload sent to Azure Service Bus when an order is placed.
    // Shared shape between the web app (publisher) and TheGala.Functions (consumers) -
    // kept as a plain DTO so either side can deserialize it without a project reference.
    public class OrderMessage
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTimeOffset PlacedAtUtc { get; set; }
    }
}
