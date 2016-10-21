using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;


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
            await CheckQueue(storageAccount);
        }

        private async Task CheckBlobs(CloudStorageAccount storageAccount)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("dnc1b");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }

        private async Task CheckQueue(CloudStorageAccount storageAccount)
        {
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("dnc1q");
            await queue.CreateIfNotExistsAsync();

            await queue.AddMessageAsync(new CloudQueueMessage("Get-Host"));
            var messages = await queue.GetMessagesAsync(5);
            foreach (var msg in messages)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;

                Console.WriteLine($"Message {msg.Id}, {msg.AsString}");

                psi.Arguments = msg.AsString;
                Process p = Process.Start(psi);
                string strOutput = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Console.WriteLine(strOutput);

                await queue.DeleteMessageAsync(msg.Id, msg.PopReceipt);
            }

        }
    }
}
