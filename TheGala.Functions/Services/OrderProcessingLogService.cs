using Azure.Data.Tables;
using TheGala.Functions.Models;

namespace TheGala.Functions.Services
{
    public class OrderProcessingLogService : IOrderProcessingLogService
    {
        private const string TableName = "OrderProcessingLog";

        private readonly TableClient _table;

        public OrderProcessingLogService(TableServiceClient tableServiceClient)
        {
            _table = tableServiceClient.GetTableClient(TableName);
            _table.CreateIfNotExists();
        }

        public async Task RecordAsync(OrderMessage order, string stage)
        {
            var entity = new OrderProcessingLogEntity
            {
                OrderId = order.OrderId,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                Stage = stage,
                ProcessedAtUtc = DateTimeOffset.UtcNow
            };

            await _table.AddEntityAsync(entity);
        }
    }
}
