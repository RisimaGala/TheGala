using System.Text;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace TheGala.Services
{
    // Talks to Azure Files. Every time a customer/product record is added, an image is
    // uploaded, or a queue message is sent, we append one line to that day's log file.
    //
    // Unlike Blob Storage, files in Azure Files have a fixed size that must be declared
    // up front. To "append", we resize the file to make room, then write only the new
    // bytes into the newly added range at the end - no need to re-upload the whole file.
    public class FileStorageService : IFileStorageService
    {
        private const string ShareName = "logs-share";

        private readonly ShareDirectoryClient _rootDirectory;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ShareServiceClient shareServiceClient, ILogger<FileStorageService> logger)
        {
            _logger = logger;

            var shareClient = shareServiceClient.GetShareClient(ShareName);

            // Creates the "logs-share" file share the first time the app runs.
            shareClient.CreateIfNotExists();

            _rootDirectory = shareClient.GetRootDirectoryClient();
        }

        public async Task LogAsync(string action)
        {
            try
            {
                var timestamp = DateTimeOffset.UtcNow;
                var line = $"{timestamp:yyyy-MM-dd HH:mm:ss} UTC - {action}{Environment.NewLine}";
                var lineBytes = Encoding.UTF8.GetBytes(line);

                // One log file per day, e.g. "log-2026-08-13.txt".
                var fileName = $"log-{timestamp:yyyy-MM-dd}.txt";
                var fileClient = _rootDirectory.GetFileClient(fileName);

                long existingLength = 0;

                if (await fileClient.ExistsAsync())
                {
                    var properties = await fileClient.GetPropertiesAsync();
                    existingLength = properties.Value.ContentLength;

                    // Grow the file so there's room for the new line at the end.
                    var newSize = existingLength + lineBytes.Length;
                    await fileClient.SetHttpHeadersAsync(new ShareFileSetHttpHeadersOptions { NewSize = newSize });
                }
                else
                {
                    // Brand new file: create it at exactly the size of the first line.
                    await fileClient.CreateAsync(lineBytes.Length);
                }

                using var stream = new MemoryStream(lineBytes);

                // Write the new line into the range we just made room for.
                await fileClient.UploadRangeAsync(new HttpRange(existingLength, lineBytes.Length), stream);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to write log entry to Azure Files.");
                throw new ApplicationException("Could not write the log entry.", ex);
            }
        }

        public async Task<List<string>> GetLogFileNamesAsync()
        {
            try
            {
                var names = new List<string>();

                await foreach (var item in _rootDirectory.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        names.Add(item.Name);
                    }
                }

                // Newest day first - file names sort naturally because of the yyyy-MM-dd format.
                return names.OrderDescending().ToList();
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to list log files from Azure Files.");
                throw new ApplicationException("Could not load the list of log files.", ex);
            }
        }

        public async Task<string> ReadLogFileAsync(string fileName)
        {
            try
            {
                var fileClient = _rootDirectory.GetFileClient(fileName);
                var download = await fileClient.DownloadAsync();

                using var reader = new StreamReader(download.Value.Content);
                return await reader.ReadToEndAsync();
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to read log file '{FileName}' from Azure Files.", fileName);
                throw new ApplicationException("Could not read the requested log file.", ex);
            }
        }
    }
}
