using Azure;
using Azure.Data.Tables;

namespace TheGala.Models
{
    // Represents a row in the "Products" Azure Table.
    public class Product : ITableEntity
    {
        // Fixed PartitionKey - all products live in the same partition for this simple app.
        public string PartitionKey { get; set; } = "Product";

        // Unique row identifier within the partition.
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        // Required by ITableEntity - managed by Azure Table Storage.
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // The actual product fields we care about.
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
