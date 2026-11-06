using AgriStream.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace AgriStream.Controllers
{
    public class TelemetryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TelemetryController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var readings = new List<TelemetryReading>();

            try
            {
                string baseUrl =
                    _configuration["AzureFunctions:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "Azure Functions BaseUrl is missing.");

                string? functionKey =
                    _configuration["AzureFunctions:FunctionKey"];

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri(new Uri(baseUrl), "api/telemetry/latest"));

                if (!string.IsNullOrWhiteSpace(functionKey))
                {
                    request.Headers.Add(
                        "x-functions-key",
                        functionKey);
                }

                using var response =
                    await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                readings =
                    await response.Content.ReadFromJsonAsync<
                        List<TelemetryReading>>()
                    ?? new List<TelemetryReading>();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve telemetry readings.");

                ViewBag.Error =
                    "Unable to retrieve telemetry data.";
            }

            return View(readings);
        }
    }
}