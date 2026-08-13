using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using TheGala.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// The connection string comes from appsettings.json ("AzureStorage:ConnectionString").
// Fill in your real Azure Storage connection string there - never hardcode it here.
var azureStorageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];

// TableServiceClient is the entry point for talking to Azure Table Storage.
// Registering it as a singleton means one client instance is reused for the app's lifetime.
builder.Services.AddSingleton(new TableServiceClient(azureStorageConnectionString));

// Our own service that wraps Table Storage operations for Customers and Products.
builder.Services.AddScoped<ITableStorageService, TableStorageService>();

// BlobServiceClient is the entry point for talking to Azure Blob Storage.
builder.Services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));

// Our own service that wraps Blob Storage operations for product images.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// QueueServiceClient is the entry point for talking to Azure Queue Storage.
builder.Services.AddSingleton(new QueueServiceClient(azureStorageConnectionString));

// Our own service that wraps Queue Storage operations for order/inventory messages.
builder.Services.AddScoped<IQueueStorageService, QueueStorageService>();

// ShareServiceClient is the entry point for talking to Azure Files.
builder.Services.AddSingleton(new ShareServiceClient(azureStorageConnectionString));

// Our own service that wraps Azure Files operations for the activity log.
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
