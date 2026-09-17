using TheGala.Models;

namespace TheGala.Services
{
    // Defines the operations our app needs against Azure Service Bus.
    //
    // Two messaging patterns, both fed by the same order event:
    //  - Queue ("order-processing"): one reliable, ordered hand-off to a single
    //    consumer (the OrderQueueProcessor function). Unlike Azure Queue Storage,
    //    Service Bus gives us at-least-once delivery with peek-lock, automatic
    //    retries and a dead-letter queue when a message keeps failing.
    //  - Topic ("order-events"): the same event fanned out to multiple independent
    //    subscribers (inventory updates, customer notifications) without the
    //    publisher knowing or caring who's listening - real-time event processing
    //    that can grow new subscribers without changing this code.
    public interface IServiceBusService
    {
        Task SendOrderToQueueAsync(OrderMessage order);

        Task PublishOrderEventAsync(OrderMessage order);

        // Peeks (non-destructively) at what's waiting on the reliable order queue.
        Task<List<QueueMessageView>> PeekQueueMessagesAsync();

        // Peeks (non-destructively) at what's waiting on a topic subscription.
        Task<List<QueueMessageView>> PeekSubscriptionMessagesAsync(string subscriptionName);
    }
}
