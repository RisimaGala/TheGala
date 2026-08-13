using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    // A minimal controller whose only job is to demonstrate sending a queue message
    // "whenever an order is placed". There's no Order table - orders aren't part of
    // the Table Storage requirements, so nothing is persisted here.
    public class OrderController : Controller
    {
        private readonly IQueueStorageService _queueStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IQueueStorageService queueStorageService,
            IFileStorageService fileStorageService,
            ILogger<OrderController> logger)
        {
            _queueStorageService = queueStorageService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: /Order/Create - shows the "place an order" form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Order/Create - sends a message like "Processing order #1023: 2 x Widget" to the queue.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string productName, int quantity)
        {
            if (string.IsNullOrWhiteSpace(productName) || quantity <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter a product name and a quantity greater than zero.");
                return View();
            }

            try
            {
                // A short random order number is enough for this demo - it isn't stored anywhere.
                var orderId = Random.Shared.Next(1000, 9999);
                var message = $"Processing order #{orderId}: {quantity} x {productName}";

                await _queueStorageService.SendMessageAsync(message);

                try
                {
                    await _fileStorageService.LogAsync($"Queue message sent: {message}");
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Order placed but failed to write log entry.");
                }

                TempData["SuccessMessage"] = $"Order #{orderId} was placed and sent to the queue.";
                return RedirectToAction("Index", "Queue");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order.");
                ModelState.AddModelError(string.Empty, "Could not place the order. Please try again.");
                return View();
            }
        }
    }
}
