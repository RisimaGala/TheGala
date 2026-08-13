using Azure;
using Azure.Data.Tables;
using TheGala.Models;

namespace TheGala.Services
{
    // Talks to Azure Table Storage for Customer and Product records.
    // A TableServiceClient (created once in Program.cs) is injected here, and we
    // ask it for a TableClient scoped to each specific table name.
    public class TableStorageService : ITableStorageService
    {
        private const string CustomersTableName = "Customers";
        private const string ProductsTableName = "Products";

        private readonly TableClient _customersTable;
        private readonly TableClient _productsTable;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(TableServiceClient tableServiceClient, ILogger<TableStorageService> logger)
        {
            _logger = logger;

            _customersTable = tableServiceClient.GetTableClient(CustomersTableName);
            _productsTable = tableServiceClient.GetTableClient(ProductsTableName);

            // CreateIfNotExists means we don't have to manually set up tables in the
            // Azure Portal first - the app creates them the first time it runs.
            _customersTable.CreateIfNotExists();
            _productsTable.CreateIfNotExists();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            try
            {
                // AddEntityAsync inserts a new row into the Customers table.
                await _customersTable.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to add customer to Table Storage.");
                throw new ApplicationException("Could not save the customer. Please try again.", ex);
            }
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var customers = new List<Customer>();

                // QueryAsync streams all entities from the table - fine for a small demo app.
                await foreach (var customer in _customersTable.QueryAsync<Customer>())
                {
                    customers.Add(customer);
                }

                return customers;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to read customers from Table Storage.");
                throw new ApplicationException("Could not load customers. Please try again.", ex);
            }
        }

        public async Task AddProductAsync(Product product)
        {
            try
            {
                await _productsTable.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to add product to Table Storage.");
                throw new ApplicationException("Could not save the product. Please try again.", ex);
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var products = new List<Product>();

                await foreach (var product in _productsTable.QueryAsync<Product>())
                {
                    products.Add(product);
                }

                return products;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to read products from Table Storage.");
                throw new ApplicationException("Could not load products. Please try again.", ex);
            }
        }
    }
}
