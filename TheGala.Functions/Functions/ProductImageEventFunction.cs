using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TheGala.Functions.Services;

namespace TheGala.Functions.Functions
{
    // Fires in real time whenever a new product image lands in the "product-images"
    // blob container - Blob Storage publishes Microsoft.Storage.BlobCreated events to
    // an Event Grid system topic natively, so no polling and no extra publishing code
    // in the web app are needed. This directly replaces the case study's "network
    // shared drive" image storage with instant, event-driven processing (e.g. a
    // thumbnail/virus-scan step could be added here without the upload path changing).
    public class ProductImageEventFunction
    {
        private readonly IActivityLogService _activityLog;
        private readonly ILogger<ProductImageEventFunction> _logger;

        public ProductImageEventFunction(IActivityLogService activityLog, ILogger<ProductImageEventFunction> logger)
        {
            _activityLog = activityLog;
            _logger = logger;
        }

        [Function("ProductImageEvent")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            _logger.LogInformation("Event Grid event received: {EventType} for {Subject}",
                eventGridEvent.EventType, eventGridEvent.Subject);

            if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
            {
                await _activityLog.LogAsync($"[Function] Product image event: {eventGridEvent.Subject} created (real-time via Event Grid).");
            }
        }
    }
}
