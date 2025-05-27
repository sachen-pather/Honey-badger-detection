using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;
using Honey_badger_detection.Configuration;

namespace Honey_badger_detection.Services
{

    public class DropboxService : IDropboxService
    {
        private readonly DropboxClient _dropboxClient;
        private readonly ILogger<DropboxService> _logger;

        public DropboxService(IOptions<DropboxSettings> dropboxSettings, ILogger<DropboxService> logger)
        {
            _logger = logger;
            _dropboxClient = new DropboxClient(dropboxSettings.Value.AccessToken);
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                _logger.LogInformation($"Uploading image to Dropbox: {fileName}");

                // Create a folder path for honey badger detections (matching frontend expectation)
                var folderPath = "/honey-badger";
                var filePath = $"{folderPath}/{fileName}";

                // Ensure the folder exists
                await EnsureFolderExistsAsync(folderPath);

                // Upload the file
                var uploadResult = await _dropboxClient.Files.UploadAsync(
                    filePath,
                    WriteMode.Add.Instance,
                    body: imageStream);

                _logger.LogInformation($"Successfully uploaded image to Dropbox: {uploadResult.PathDisplay}");
                return uploadResult.PathDisplay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading image to Dropbox: {fileName}");
                throw;
            }
        }

        private async Task EnsureFolderExistsAsync(string folderPath)
        {
            try
            {
                await _dropboxClient.Files.GetMetadataAsync(folderPath);
            }
            catch (ApiException<GetMetadataError> ex) when (ex.ErrorResponse.IsPath && ex.ErrorResponse.AsPath.Value.IsNotFound)
            {
                // Folder doesn't exist, create it
                _logger.LogInformation($"Creating folder: {folderPath}");
                await _dropboxClient.Files.CreateFolderV2Async(folderPath);
            }
        }

        public void Dispose()
        {
            _dropboxClient?.Dispose();
        }
    }
}