using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    // Shows what's currently waiting on the Service Bus queue and topic subscriptions -
    // the Service Bus equivalent of QueueController, so the reliable/real-time
    // messaging path (Part 2) has the same kind of "did it actually send?" visibility
    // as the Azure Queue Storage path (Part 1).
    public class EventsController : Controller
    {
        private readonly IServiceBusService _serviceBusService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IServiceBusService serviceBusService, ILogger<EventsController> logger)
        {
            _serviceBusService = serviceBusService;
            _logger = logger;
        }

        // GET: /Events - shows messages waiting on the "order-processing" queue and on
        // each "order-events" topic subscription (inventory-updates, customer-notifications).
        public async Task<IActionResult> Index()
        {
            var model = new EventsOverview();

            try
            {
                model.QueueMessages = await _serviceBusService.PeekQueueMessagesAsync();
                model.InventoryMessages = await _serviceBusService.PeekSubscriptionMessagesAsync("inventory-updates");
                model.NotificationMessages = await _serviceBusService.PeekSubscriptionMessagesAsync("customer-notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Service Bus messages.");
                TempData["ErrorMessage"] = "Could not load messages from Azure Service Bus.";
            }

            return View(model);
        }
    }
}
