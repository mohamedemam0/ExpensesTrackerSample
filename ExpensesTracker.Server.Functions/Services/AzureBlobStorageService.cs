using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesTracker.Server.Functions.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly string _connectionString;

        public AzureBlobStorageService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task DeleteFileAsync(string filePath)
        {
            var container = await GetContainerAsync();
            var fileName = Path.GetFileName(filePath);
            var blob = container.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string> SaveFileAsync(Stream stream, string fileName)
        {
            var container = await GetContainerAsync();
            var extension = Path.GetExtension(fileName).ToLower();
            var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            if (!validExtensions.Contains(extension))
                throw new NotSupportedException($"This extensions: {extension} is not supported.");

            var nameOnly = Path.GetFileNameWithoutExtension(fileName);
            var newFileName = $"{nameOnly}-{Guid.NewGuid()}{extension}";

            var blob = container.GetBlobClient(newFileName);
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(extension)
                }
            });

            return blob.Uri.AbsoluteUri;
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => throw new NotSupportedException($"This extensions: {extension} is not supported.")
            };
        }

        private async Task<BlobContainerClient> GetContainerAsync()
        {
            var blobClient = new BlobServiceClient(_connectionString);
            var container = blobClient.GetBlobContainerClient("attachments");
            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}
