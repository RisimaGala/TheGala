using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TheGala.Functions.Models;
using TheGala.Functions.Services;

namespace TheGala.Functions.Functions
{
    // One of two independent subscribers to the "order-events" topic - reacts to the
    // same OrderPlaced event as CustomerNotificationFunction without either function
    // knowing the other exists, which is the point of pub/sub fan-out: new subscribers
    // (e.g. a future analytics function) can be added without touching the publisher.
    public class InventoryUpdateFunction
    {
        private readonly IOrderProcessingLogService _processingLog;
        private readonly IActivityLogService _activityLog;
        private readonly ILogger<InventoryUpdateFunction> _logger;

        public InventoryUpdateFunction(
            IOrderProcessingLogService processingLog,
            IActivityLogService activityLog,
            ILogger<InventoryUpdateFunction> logger)
        {
            _processingLog = processingLog;
            _activityLog = activityLog;
            _logger = logger;
        }

        [Function("InventoryUpdate")]
        public async Task Run(
            [ServiceBusTrigger("order-events", "inventory-updates", Connection = "ServiceBusConnection")] string messageBody)
        {
            var order = JsonSerializer.Deserialize<OrderMessage>(messageBody)
                ?? throw new InvalidOperationException("Order message could not be deserialized.");

            _logger.LogInformation("Reserving stock for order #{OrderId}: {Quantity} x {ProductName}",
                order.OrderId, order.Quantity, order.ProductName);

            await _processingLog.RecordAsync(order, "InventoryUpdated");
            await _activityLog.LogAsync($"[Function] Inventory reserved {order.Quantity} x {order.ProductName} for order #{order.OrderId}.");
        }
    }
}
