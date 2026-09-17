using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TheGala.Functions.Models;
using TheGala.Functions.Services;

namespace TheGala.Functions.Functions
{
    // Reliable, single-consumer processing of every order. The Service Bus queue gives
    // this function peek-lock semantics: if it throws, the message becomes visible again
    // for another attempt (up to host.json's max delivery count) instead of being lost,
    // and after repeated failures it's moved to the dead-letter queue instead of
    // blocking the queue forever - the reliability the legacy middleware couldn't offer.
    public class OrderQueueProcessorFunction
    {
        private readonly IOrderProcessingLogService _processingLog;
        private readonly IActivityLogService _activityLog;
        private readonly ILogger<OrderQueueProcessorFunction> _logger;

        public OrderQueueProcessorFunction(
            IOrderProcessingLogService processingLog,
            IActivityLogService activityLog,
            ILogger<OrderQueueProcessorFunction> logger)
        {
            _processingLog = processingLog;
            _activityLog = activityLog;
            _logger = logger;
        }

        [Function("OrderQueueProcessor")]
        public async Task Run(
            [ServiceBusTrigger("order-processing", Connection = "ServiceBusConnection")] string messageBody)
        {
            var order = JsonSerializer.Deserialize<OrderMessage>(messageBody)
                ?? throw new InvalidOperationException("Order message could not be deserialized.");

            _logger.LogInformation("Processing order #{OrderId}: {Quantity} x {ProductName}",
                order.OrderId, order.Quantity, order.ProductName);

            await _processingLog.RecordAsync(order, "QueueProcessed");
            await _activityLog.LogAsync($"[Function] Order #{order.OrderId} processed from order-processing queue.");
        }
    }
}
