using System.Text;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace TheGala.Functions.Services
{
    // Writes to the exact same "logs-share" file share and daily-file naming as
    // TheGala's FileStorageService, so function activity shows up in the web app's
    // existing /Log pages alongside orders/uploads instead of needing its own viewer.
    public class ActivityLogService : IActivityLogService
    {
        private const string ShareName = "logs-share";

        private readonly ShareDirectoryClient _rootDirectory;

        public ActivityLogService(ShareServiceClient shareServiceClient)
        {
            var shareClient = shareServiceClient.GetShareClient(ShareName);
            shareClient.CreateIfNotExists();
            _rootDirectory = shareClient.GetRootDirectoryClient();
        }

        public async Task LogAsync(string action)
        {
            var timestamp = DateTimeOffset.UtcNow;
            var line = $"{timestamp:yyyy-MM-dd HH:mm:ss} UTC - {action}{Environment.NewLine}";
            var lineBytes = Encoding.UTF8.GetBytes(line);

            var fileName = $"log-{timestamp:yyyy-MM-dd}.txt";
            var fileClient = _rootDirectory.GetFileClient(fileName);

            long existingLength = 0;

            if (await fileClient.ExistsAsync())
            {
                var properties = await fileClient.GetPropertiesAsync();
                existingLength = properties.Value.ContentLength;

                var newSize = existingLength + lineBytes.Length;
                await fileClient.SetHttpHeadersAsync(new ShareFileSetHttpHeadersOptions { NewSize = newSize });
            }
            else
            {
                await fileClient.CreateAsync(lineBytes.Length);
            }

            using var stream = new MemoryStream(lineBytes);
            await fileClient.UploadRangeAsync(new Azure.HttpRange(existingLength, lineBytes.Length), stream);
        }
    }
}
