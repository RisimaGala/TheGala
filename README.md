TheGala – Azure Storage Web Application

An ASP.NET Core MVC web application built for ABC Retail, demonstrating the use of four core Azure Storage services to manage customer/product data, product images, order processing, and system logs.

Built as part of a cloud development module assignment (Project 1).

Overview

ABC Retail needed a simple, scalable way to manage:

Customer and product records
Product images
Order and inventory processing messages
Application activity logs

This project demonstrates how Azure Storage services can replace legacy on-premises infrastructure for exactly these needs.

Azure Services Used
Service	Purpose
Azure Table Storage	Stores customer profiles and product information
Azure Blob Storage	Stores and serves product images
Azure Queue Storage	Handles order and inventory processing messages (orders-queue)
Azure File Storage	Stores application log files (logs-share)
Tech Stack
ASP.NET Core MVC (.NET 8)
Azure.Data.Tables
Azure.Storage.Blobs
Azure.Storage.Queues
Azure.Storage.Files.Shares
Bootstrap (default ASP.NET Core MVC styling)
Project Structure
TheGala/
├── Controllers/       # MVC controllers for each feature
├── Models/             # Data models (Customer, Product, etc.)
├── Services/           # Service classes wrapping each Azure Storage operation
├── Views/               # Razor views for each page
├── Program.cs        # App startup and Azure service registration
├── appsettings.json  # Configuration (connection string)
Getting Started
Prerequisites
Visual Studio 2022
.NET 8 SDK
An Azure account with an active Storage Account
Setup
Clone the repository:
   git clone https://github.com/RisimagGala/TheGala.git
Open TheGala.sln in Visual Studio 2022.
In appsettings.json (or appsettings.Development.json), add your Azure Storage connection string:
json
   "AzureStorage": {
     "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=thegala03;AccountKey=/9iyaEGWKKFoVwux/JqdDYjfiLxVgEJYPo7Z7Aqarr7JdbiPTVGgAe4UwHbvDGip7Z34I7Zqhg8F+ASt9LkmBA==;EndpointSuffix=core.windows.net"
   }
Run the application (F5). The required table, blob container, queue, and file share are created automatically on first use if they don't already exist.
Azure Resources Used
Table Storage: Customer and Product tables
Blob Container: product-images
Queue: orders-queue
File Share: logs-share
Features
Add and view customer and product records (Table Storage)
Upload and display product images (Blob Storage)
Send and view order/inventory processing messages (Queue Storage)
View application log entries (Azure Files)
Deployment

The application is deployed to Azure App Service and accessible at:

http://<ST10380589>.azurewebsites.net
Author

Risima-Ra-Rifuwo Gala - Rosebank International, Advanced Diploma in Application Development

License

This project was developed for academic purposes as part of a university module assignment.
