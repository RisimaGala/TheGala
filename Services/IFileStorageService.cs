namespace TheGala.Services
{
    // Defines the operations our app needs against Azure Files.
    public interface IFileStorageService
    {
        // Appends a single "timestamp - action" line to today's log file.
        Task LogAsync(string action);

        // Lists the names of all log files currently stored on the share.
        Task<List<string>> GetLogFileNamesAsync();

        // Reads the full text content of one log file.
        Task<string> ReadLogFileAsync(string fileName);
    }
}
