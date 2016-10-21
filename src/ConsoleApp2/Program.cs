using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace ConsoleApp2
{
    public class Program
    {
        static public IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            Console.WriteLine("Executing ..");
            new Program().Execute().Wait();
        }

        public async Task Execute()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("config.json");

            Configuration = configurationBuilder.Build();
            string connectionString = Program.Configuration["MicrosoftAzureStorage:ConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            await CheckBlobs(storageAccount);
        }

        private async Task CheckBlobs(CloudStorageAccount storageAccount)
        {
            // Create a blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get a reference to a container named “my-new-container.”
            CloudBlobContainer container = blobClient.GetContainerReference("dnc1blobs");

            // If “mycontainer” doesn’t exist, create it.
            await container.CreateIfNotExistsAsync();

            await container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }
    }
}
