using Azure;
using Azure.Storage.Queues;
using TheGala.Models;

namespace TheGala.Services
{
    // Talks to Azure Queue Storage. Used to notify the (imaginary) order/inventory
    // processing system whenever an order is placed or a product image is uploaded.
    public class QueueStorageService : IQueueStorageService
    {
        private const string QueueName = "orders-queue";

        private readonly QueueClient _queueClient;
        private readonly ILogger<QueueStorageService> _logger;

        public QueueStorageService(QueueServiceClient queueServiceClient, ILogger<QueueStorageService> logger)
        {
            _logger = logger;

            _queueClient = queueServiceClient.GetQueueClient(QueueName);

            // Creates the "orders-queue" queue the first time the app runs.
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                await _queueClient.SendMessageAsync(message);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to send message to Queue Storage.");
                throw new ApplicationException("Could not send the queue message. Please try again.", ex);
            }
        }

        public async Task<List<QueueMessageView>> GetMessagesAsync()
        {
            try
            {
                // PeekMessages reads messages without making them invisible or deleting them,
                // so this "view" page never disturbs whatever would normally process the queue.
                // Azure Queue Storage allows peeking at up to 32 messages per call.
                var response = await _queueClient.PeekMessagesAsync(maxMessages: 32);

                return response.Value
                    .Select(m => new QueueMessageView
                    {
                        Text = m.MessageText,
                        InsertedOn = m.InsertedOn
                    })
                    .ToList();
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to peek messages from Queue Storage.");
                throw new ApplicationException("Could not load queue messages. Please try again.", ex);
            }
        }
    }
}
