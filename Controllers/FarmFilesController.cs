using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AgriStream.Controllers
{
    public class FarmFilesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FarmFilesController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                TempData["Error"] = "File cannot exceed 10 MB.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string baseUrl =
                    _configuration["AzureFunctions:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "Azure Functions URL is missing.");

                string? functionKey =
                    _configuration["AzureFunctions:FunctionKey"];

                using var formData = new MultipartFormDataContent();

                await using var stream = file.OpenReadStream();

                using var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        string.IsNullOrWhiteSpace(file.ContentType)
                            ? "application/octet-stream"
                            : file.ContentType);

                formData.Add(
                    fileContent,
                    "file",
                    Path.GetFileName(file.FileName));

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    new Uri(new Uri(baseUrl), "api/upload"));

                request.Content = formData;

                if (!string.IsNullOrWhiteSpace(functionKey))
                {
                    request.Headers.Add(
                        "x-functions-key",
                        functionKey);
                }

                using var response =
                    await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "The file could not be uploaded.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] =
                    "File uploaded successfully to Azure Blob Storage.";
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not connect to Azure Functions.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An unexpected error occurred during upload.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}