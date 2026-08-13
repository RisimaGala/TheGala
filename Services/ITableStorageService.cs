using TheGala.Models;

namespace TheGala.Services
{
    // Defines the operations our app needs against Azure Table Storage.
    // Having an interface makes it easy to swap the implementation later (e.g. for testing).
    public interface ITableStorageService
    {
        Task AddCustomerAsync(Customer customer);
        Task<List<Customer>> GetCustomersAsync();

        Task AddProductAsync(Product product);
        Task<List<Product>> GetProductsAsync();
    }
}
