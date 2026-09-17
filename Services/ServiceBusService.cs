using System.Text.Json;
using Azure;
using Azure.Messaging.ServiceBus;
using TheGala.Models;

namespace TheGala.Services
{
    public class ServiceBusService : IServiceBusService, IAsyncDisposable
    {
        private const string OrderQueueName = "order-processing";
        private const string OrderTopicName = "order-events";

        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _queueSender;
        private readonly ServiceBusSender _topicSender;
        private readonly ILogger<ServiceBusService> _logger;

        public ServiceBusService(ServiceBusClient client, ILogger<ServiceBusService> logger)
        {
            _client = client;
            _logger = logger;

            _queueSender = _client.CreateSender(OrderQueueName);
            _topicSender = _client.CreateSender(OrderTopicName);
        }

        public async Task SendOrderToQueueAsync(OrderMessage order)
        {
            try
            {
                var message = CreateMessage(order, "OrderPlaced");
                await _queueSender.SendMessageAsync(message);
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Failed to send order message to the '{Queue}' queue.", OrderQueueName);
                throw new ApplicationException("Could not send the order for processing. Please try again.", ex);
            }
        }

        public async Task PublishOrderEventAsync(OrderMessage order)
        {
            try
            {
                var message = CreateMessage(order, "OrderPlaced");
                await _topicSender.SendMessageAsync(message);
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Failed to publish order event to the '{Topic}' topic.", OrderTopicName);
                throw new ApplicationException("Could not publish the order event. Please try again.", ex);
            }
        }

        public async Task<List<QueueMessageView>> PeekQueueMessagesAsync()
        {
            await using var receiver = _client.CreateReceiver(OrderQueueName);
            return await PeekAsync(receiver, OrderQueueName);
        }

        public async Task<List<QueueMessageView>> PeekSubscriptionMessagesAsync(string subscriptionName)
        {
            await using var receiver = _client.CreateReceiver(OrderTopicName, subscriptionName);
            return await PeekAsync(receiver, $"{OrderTopicName}/{subscriptionName}");
        }

        private async Task<List<QueueMessageView>> PeekAsync(ServiceBusReceiver receiver, string sourceName)
        {
            try
            {
                var peeked = await receiver.PeekMessagesAsync(maxMessages: 32);

                return peeked
                    .Select(m => new QueueMessageView
                    {
                        Text = m.Body.ToString(),
                        InsertedOn = m.EnqueuedTime
                    })
                    .ToList();
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Failed to peek messages from '{Source}'.", sourceName);
                throw new ApplicationException("Could not load Service Bus messages. Please try again.", ex);
            }
        }

        private static ServiceBusMessage CreateMessage(OrderMessage order, string eventType)
        {
            var json = JsonSerializer.Serialize(order);

            return new ServiceBusMessage(json)
            {
                ContentType = "application/json",
                Subject = eventType,
                MessageId = $"{order.OrderId}-{Guid.NewGuid():N}"
            };
        }

        public async ValueTask DisposeAsync()
        {
            await _queueSender.DisposeAsync();
            await _topicSender.DisposeAsync();
        }
    }
}
