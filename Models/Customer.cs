using Azure;
using Azure.Data.Tables;

namespace TheGala.Models
{
    // Represents a row in the "Customers" Azure Table.
    // Azure Table Storage requires every entity to implement ITableEntity,
    // which is what gives it PartitionKey, RowKey, Timestamp and ETag.
    public class Customer : ITableEntity
    {
        // PartitionKey groups related rows together for fast lookups.
        // We use a fixed value ("Customer") since we don't need extra grouping here.
        public string PartitionKey { get; set; } = "Customer";

        // RowKey uniquely identifies a row within a partition.
        // A new GUID guarantees it never clashes with another customer.
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        // Required by ITableEntity - Azure sets these automatically, we don't need to.
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // The actual customer fields we care about.
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
