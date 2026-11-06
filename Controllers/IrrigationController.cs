using AgriStream.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace AgriStream.Controllers
{
    public class IrrigationController : Controller
    {
        private readonly AgriStreamDBContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IrrigationController> _logger;

        public IrrigationController(
            AgriStreamDBContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<IrrigationController> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var devices = await _context.DeviceRegistries
                .AsNoTracking()
                .Where(d => d.DeviceType == "IrrigationController")
                .OrderBy(d => d.DeviceId)
                .ToListAsync();

            return View(devices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Trigger(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                TempData["Error"] = "Invalid device selected.";
                return RedirectToAction(nameof(Index));
            }

            var device = await _context.DeviceRegistries
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DeviceId == deviceId &&
                    d.DeviceType == "IrrigationController");

            if (device == null)
            {
                TempData["Error"] = "Irrigation controller not found.";
                return RedirectToAction(nameof(Index));
            }

            if (device.OperationalStatus == "Active")
            {
                TempData["Error"] =
                    "This irrigation controller is already active.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                string baseUrl =
                    _configuration["AzureFunctions:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "Azure Functions BaseUrl is missing.");

                string? functionKey =
                    _configuration["AzureFunctions:FunctionKey"];

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(new Uri(baseUrl), "api/irrigation/trigger"));

                if (!string.IsNullOrWhiteSpace(functionKey))
                {
                    request.Headers.Add(
                        "x-functions-key",
                        functionKey);
                }

                request.Content = JsonContent.Create(new
                {
                    deviceId = device.DeviceId,
                    action = "START"
                });

                using var response =
                    await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        $"Irrigation command queued for {device.DeviceName}. " +
                        "Refresh the page to check its status.";
                }
                else
                {
                    _logger.LogWarning(
                        "Irrigation function returned HTTP {StatusCode} for {DeviceId}.",
                        (int)response.StatusCode,
                        device.DeviceId);

                    TempData["Error"] =
                        "Unable to queue the irrigation command.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering irrigation for {DeviceId}.",
                    deviceId);

                TempData["Error"] =
                    "Could not communicate with the irrigation service.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}