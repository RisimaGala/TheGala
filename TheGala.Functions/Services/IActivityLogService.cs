namespace TheGala.Functions.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(string action);
    }
}
