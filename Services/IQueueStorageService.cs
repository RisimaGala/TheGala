using TheGala.Models;

namespace TheGala.Services
{
    // Defines the operations our app needs against Azure Queue Storage.
    public interface IQueueStorageService
    {
        // Sends a plain-text message onto the "orders-queue" queue.
        Task SendMessageAsync(string message);

        // Peeks at the current messages without removing them from the queue,
        // so the "view queue messages" page doesn't interfere with real processing.
        Task<List<QueueMessageView>> GetMessagesAsync();
    }
}
