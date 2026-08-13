using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    public class ProductController : Controller
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            ITableStorageService tableStorageService,
            IFileStorageService fileStorageService,
            ILogger<ProductController> logger)
        {
            _tableStorageService = tableStorageService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: /Product - shows the list of products stored in Azure Table Storage.
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _tableStorageService.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products.");
                TempData["ErrorMessage"] = "Could not load products from Azure Table Storage.";
                return View(new List<Product>());
            }
        }

        // GET: /Product/Create - shows the empty form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create - saves the new product to Azure Table Storage.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            try
            {
                await _tableStorageService.AddProductAsync(product);

                try
                {
                    await _fileStorageService.LogAsync($"Product added: {product.Name}");
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Product added but failed to write log entry.");
                }

                TempData["SuccessMessage"] = $"Product '{product.Name}' was added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product.");
                ModelState.AddModelError(string.Empty, "Could not save the product. Please try again.");
                return View(product);
            }
        }
    }
}
