using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ITableStorageService tableStorageService,
            IFileStorageService fileStorageService,
            ILogger<CustomerController> logger)
        {
            _tableStorageService = tableStorageService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: /Customer - shows the list of customers stored in Azure Table Storage.
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _tableStorageService.GetCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers.");
                TempData["ErrorMessage"] = "Could not load customers from Azure Table Storage.";
                return View(new List<Customer>());
            }
        }

        // GET: /Customer/Create - shows the empty form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create - saves the new customer to Azure Table Storage.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            try
            {
                await _tableStorageService.AddCustomerAsync(customer);

                // A logging failure shouldn't undo a successful save, so it's handled separately.
                try
                {
                    await _fileStorageService.LogAsync($"Customer added: {customer.Name}");
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Customer added but failed to write log entry.");
                }

                TempData["SuccessMessage"] = $"Customer '{customer.Name}' was added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer.");
                ModelState.AddModelError(string.Empty, "Could not save the customer. Please try again.");
                return View(customer);
            }
        }
    }
}
