namespace TheGala.Services
{
    // Defines the operations our app needs against Azure Blob Storage.
    public interface IBlobStorageService
    {
        // Uploads an image and returns the blob name it was stored under.
        Task<string> UploadImageAsync(IFormFile file);

        // Lists the names of all blobs currently in the container.
        Task<List<string>> GetImageNamesAsync();

        // Downloads a blob's content so it can be streamed back to the browser.
        Task<(Stream Content, string ContentType)> DownloadImageAsync(string blobName);
    }
}
