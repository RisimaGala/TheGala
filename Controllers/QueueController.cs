using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    public class QueueController : Controller
    {
        private readonly IQueueStorageService _queueStorageService;
        private readonly ILogger<QueueController> _logger;

        public QueueController(IQueueStorageService queueStorageService, ILogger<QueueController> logger)
        {
            _queueStorageService = queueStorageService;
            _logger = logger;
        }

        // GET: /Queue - shows the messages currently waiting on "orders-queue".
        public async Task<IActionResult> Index()
        {
            try
            {
                var messages = await _queueStorageService.GetMessagesAsync();
                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading queue messages.");
                TempData["ErrorMessage"] = "Could not load messages from Azure Queue Storage.";
                return View(new List<QueueMessageView>());
            }
        }
    }
}
