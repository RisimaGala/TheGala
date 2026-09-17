using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TheGala.Functions.Models;
using TheGala.Functions.Services;

namespace TheGala.Functions.Functions
{
    // The other subscriber to "order-events" - simulates sending the customer a
    // confirmation (a real deployment would call an email/SMS provider here).
    public class CustomerNotificationFunction
    {
        private readonly IOrderProcessingLogService _processingLog;
        private readonly IActivityLogService _activityLog;
        private readonly ILogger<CustomerNotificationFunction> _logger;

        public CustomerNotificationFunction(
            IOrderProcessingLogService processingLog,
            IActivityLogService activityLog,
            ILogger<CustomerNotificationFunction> logger)
        {
            _processingLog = processingLog;
            _activityLog = activityLog;
            _logger = logger;
        }

        [Function("CustomerNotification")]
        public async Task Run(
            [ServiceBusTrigger("order-events", "customer-notifications", Connection = "ServiceBusConnection")] string messageBody)
        {
            var order = JsonSerializer.Deserialize<OrderMessage>(messageBody)
                ?? throw new InvalidOperationException("Order message could not be deserialized.");

            _logger.LogInformation("Notifying customer about order #{OrderId}", order.OrderId);

            await _processingLog.RecordAsync(order, "CustomerNotified");
            await _activityLog.LogAsync($"[Function] Notification sent: your order #{order.OrderId} ({order.Quantity} x {order.ProductName}) has been received.");
        }
    }
}
