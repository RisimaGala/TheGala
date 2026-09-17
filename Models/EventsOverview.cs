namespace TheGala.Models
{
    // View model for the /Events page: a snapshot of what's currently sitting on the
    // Service Bus queue and each topic subscription.
    public class EventsOverview
    {
        public List<QueueMessageView> QueueMessages { get; set; } = new();
        public List<QueueMessageView> InventoryMessages { get; set; } = new();
        public List<QueueMessageView> NotificationMessages { get; set; } = new();
    }
}
