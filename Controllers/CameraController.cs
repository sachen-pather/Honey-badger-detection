using Microsoft.AspNetCore.Mvc;
using Honey_badger_detection.Services;
using Honey_badger_detection.Models;
using System.Diagnostics;

namespace Honey_badger_detection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly ICustomVisionService _customVisionService;
        private readonly IDropboxService _dropboxService;
        private readonly ILogger<CameraController> _logger;

        public CameraController(
            ICustomVisionService customVisionService,
            IDropboxService dropboxService,
            ILogger<CameraController> logger)
        {
            _customVisionService = customVisionService;
            _dropboxService = dropboxService;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalysisResult>> AnalyzeImage(IFormFile image)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                _logger.LogInformation("Image analysis request received");

                if (image == null || image.Length == 0)
                    return BadRequest("No image provided");

                _logger.LogInformation($"Analyzing image ({image.Length} bytes)");

                // Create a memory stream to hold the image data for both analysis and potential upload
                using var imageStream = new MemoryStream();
                await image.CopyToAsync(imageStream);
                imageStream.Position = 0;

                // Analyze the image
                var result = await _customVisionService.AnalyzeImageAsync(image);

                var honeyBadgerPrediction = result.Predictions
                    .FirstOrDefault(p => p.TagName.ToLower() == "honey-badger");

                var analysisResult = new AnalysisResult
                {
                    IsHoneyBadger = honeyBadgerPrediction?.Probability > 0.7,
                    Confidence = honeyBadgerPrediction?.Probability ?? 0,
                    FullResponse = result,
                    ProcessingTimeMs = sw.ElapsedMilliseconds
                };

                _logger.LogInformation($"Analysis completed in {sw.ElapsedMilliseconds}ms. " +
                    $"Honey badger detected: {analysisResult.IsHoneyBadger}, " +
                    $"Confidence: {analysisResult.Confidence:P2}");

                // If honey badger is detected, upload to Dropbox
                if (analysisResult.IsHoneyBadger)
                {
                    try
                    {
                        // Get the file extension from the original image
                        var fileExtension = Path.GetExtension(image.FileName) ?? ".jpg";
                        if (string.IsNullOrEmpty(fileExtension))
                            fileExtension = ".jpg";

                        var fileName = $"honey_badger_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";

                        // Reset stream position for upload
                        imageStream.Position = 0;

                        var dropboxPath = await _dropboxService.UploadImageAsync(imageStream, fileName);

                        _logger.LogInformation($"Honey badger image uploaded to Dropbox: {dropboxPath}");

                        // Add the Dropbox path to the analysis result
                        analysisResult.DropboxPath = dropboxPath;
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Failed to upload image to Dropbox, but analysis was successful");
                        // Don't fail the entire request if Dropbox upload fails
                        analysisResult.DropboxUploadError = uploadEx.Message;
                    }
                }

                return Ok(analysisResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image");
                return StatusCode(500, new { error = "Error processing image", message = ex.Message });
            }
        }
    }
}