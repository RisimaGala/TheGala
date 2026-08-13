namespace TheGala.Models
{
    // Simple view model for displaying a peeked message from Azure Queue Storage.
    public class QueueMessageView
    {
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset? InsertedOn { get; set; }
    }
}
