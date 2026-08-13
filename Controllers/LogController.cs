using Microsoft.AspNetCore.Mvc;
using TheGala.Services;

namespace TheGala.Controllers
{
    public class LogController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<LogController> _logger;

        public LogController(IFileStorageService fileStorageService, ILogger<LogController> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: /Log - lists the log files stored on the "logs-share" Azure Files share.
        public async Task<IActionResult> Index()
        {
            try
            {
                var fileNames = await _fileStorageService.GetLogFileNamesAsync();
                return View(fileNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading log file list.");
                TempData["ErrorMessage"] = "Could not load the list of log files from Azure Files.";
                return View(new List<string>());
            }
        }

        // GET: /Log/Details?fileName=... - shows the contents of one log file.
        public async Task<IActionResult> Details(string fileName)
        {
            try
            {
                var content = await _fileStorageService.ReadLogFileAsync(fileName);
                ViewData["FileName"] = fileName;
                return View(model: content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file '{FileName}'.", fileName);
                TempData["ErrorMessage"] = $"Could not read log file '{fileName}'.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
