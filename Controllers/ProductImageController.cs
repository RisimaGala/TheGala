using Microsoft.AspNetCore.Mvc;
using TheGala.Models;
using TheGala.Services;

namespace TheGala.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IQueueStorageService _queueStorageService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProductImageController> _logger;

        public ProductImageController(
            IBlobStorageService blobStorageService,
            IQueueStorageService queueStorageService,
            IFileStorageService fileStorageService,
            ILogger<ProductImageController> logger)
        {
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: /ProductImage - the gallery of uploaded product images.
        public async Task<IActionResult> Index()
        {
            try
            {
                var names = await _blobStorageService.GetImageNamesAsync();

                // Build a view model per image, pointing at our own streaming action below.
                var images = names
                    .Select(name => new ProductImage
                    {
                        Name = name,
                        Url = Url.Action(nameof(Stream), new { name })!
                    })
                    .ToList();

                return View(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product images.");
                TempData["ErrorMessage"] = "Could not load product images from Azure Blob Storage.";
                return View(new List<ProductImage>());
            }
        }

        // GET: /ProductImage/Upload - shows the upload form.
        public IActionResult Upload()
        {
            return View();
        }

        // POST: /ProductImage/Upload - uploads the selected file to the "product-images" container.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose an image file to upload.");
                return View();
            }

            try
            {
                var blobName = await _blobStorageService.UploadImageAsync(file);

                // Notify the (imaginary) processing system that a new image has arrived.
                // A queue failure here shouldn't make a successful upload look like an error,
                // so it's caught and logged separately rather than re-thrown.
                try
                {
                    await _queueStorageService.SendMessageAsync($"Image uploaded: {blobName}");
                }
                catch (Exception queueEx)
                {
                    _logger.LogWarning(queueEx, "Image uploaded but failed to send queue notification.");
                }

                try
                {
                    await _fileStorageService.LogAsync($"Image uploaded: {blobName}");
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Image uploaded but failed to write log entry.");
                }

                TempData["SuccessMessage"] = $"Image '{blobName}' was uploaded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image.");
                ModelState.AddModelError(string.Empty, "Could not upload the image. Please try again.");
                return View();
            }
        }

        // GET: /ProductImage/Stream?name=... - streams the raw image bytes back to the browser.
        // This is what the <img> tags in the gallery point to, since the blob container is private.
        public async Task<IActionResult> Stream(string name)
        {
            try
            {
                var (content, contentType) = await _blobStorageService.DownloadImageAsync(name);
                return File(content, string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming product image '{Name}'.", name);
                return NotFound();
            }
        }
    }
}
