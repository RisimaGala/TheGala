using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TheGala.Services
{
    // Talks to Azure Blob Storage for product images.
    // The container is kept private - we don't rely on "public blob access" being
    // enabled on the storage account (many student subscriptions block it by policy).
    // Instead, images are streamed back to the browser through our own controller action.
    public class BlobStorageService : IBlobStorageService
    {
        private const string ContainerName = "product-images";

        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            _containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            // Creates the "product-images" container the first time the app runs.
            // No public access type is passed, so the container stays private.
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Prefix with a GUID so two people uploading "photo.jpg" don't overwrite each other.
                var blobName = $"{Guid.NewGuid()}-{file.FileName}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();

                // Keep the original content type (e.g. image/png) so the browser renders it correctly later.
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                };

                await blobClient.UploadAsync(stream, uploadOptions);

                return blobName;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to upload image to Blob Storage.");
                throw new ApplicationException("Could not upload the image. Please try again.", ex);
            }
        }

        public async Task<List<string>> GetImageNamesAsync()
        {
            try
            {
                var names = new List<string>();

                await foreach (var blobItem in _containerClient.GetBlobsAsync())
                {
                    names.Add(blobItem.Name);
                }

                return names;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to list images from Blob Storage.");
                throw new ApplicationException("Could not load product images. Please try again.", ex);
            }
        }

        public async Task<(Stream Content, string ContentType)> DownloadImageAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Streaming download avoids loading the whole image into memory at once.
                var download = await blobClient.DownloadStreamingAsync();

                return (download.Value.Content, download.Value.Details.ContentType);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to download image '{BlobName}' from Blob Storage.", blobName);
                throw new ApplicationException("Could not load the requested image.", ex);
            }
        }
    }
}
