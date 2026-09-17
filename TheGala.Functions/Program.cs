using Azure.Data.Tables;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheGala.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Same storage account the web app uses, so these functions write into the
        // same "OrderProcessingLog" table and "logs-share" file share the app already
        // has views for - no separate dashboard needed to see what got processed.
        var storageConnectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");

        services.AddSingleton(new TableServiceClient(storageConnectionString));
        services.AddSingleton(new ShareServiceClient(storageConnectionString));

        services.AddScoped<IOrderProcessingLogService, OrderProcessingLogService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
    })
    .Build();

host.Run();
