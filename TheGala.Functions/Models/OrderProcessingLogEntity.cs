using Azure;
using Azure.Data.Tables;

namespace TheGala.Functions.Models
{
    // A row in the "OrderProcessingLog" Azure Table - one row per processing stage
    // a function completed for an order, so the pipeline's work is auditable
    // (e.g. via Azure Storage Explorer or the portal) without reading function logs.
    public class OrderProcessingLogEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "OrderProcessing";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public int OrderId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        // Which function/stage produced this row, e.g. "QueueProcessed", "InventoryUpdated", "CustomerNotified".
        public string Stage { get; set; } = string.Empty;
        public DateTimeOffset ProcessedAtUtc { get; set; }
    }
}
