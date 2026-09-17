using TheGala.Functions.Models;

namespace TheGala.Functions.Services
{
    public interface IOrderProcessingLogService
    {
        Task RecordAsync(OrderMessage order, string stage);
    }
}
