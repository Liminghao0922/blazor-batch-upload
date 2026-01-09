using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace Api
{
    public class FileUploadFunction
    {
        private readonly ILogger<FileUploadFunction> _logger;

        public FileUploadFunction(ILogger<FileUploadFunction> logger)
        {
            _logger = logger;
        }

        [Function("UploadFiles")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "files/upload")] HttpRequest req)
        {
            _logger.LogInformation("Processing file upload request.");

            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Request must be multipart/form-data");
            }

            var form = await req.ReadFormAsync();
            var files = form.Files;

            if (files.Count == 0)
            {
                return new BadRequestObjectResult("No files received.");
            }

            var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                 connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            }
            
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("uploads");
            await containerClient.CreateIfNotExistsAsync();

            var uploadResults = new List<string>();

            foreach (var file in files)
            {
                // Simple file name usage - in production, consider guid or sanitization
                var blobClient = containerClient.GetBlobClient(file.FileName);
                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, true);
                uploadResults.Add(file.FileName);
                _logger.LogInformation($"Uploaded {file.FileName}");
            }

            return new OkObjectResult(new { message = "Upload successful", files = uploadResults });
        }

        [Function("ListFiles")]
        public async Task<IActionResult> ListFiles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "files/list")] HttpRequest req)
        {
            _logger.LogInformation("Listing files.");

            var connectionString = Environment.GetEnvironmentVariable("BlobStorageConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("uploads");
            await containerClient.CreateIfNotExistsAsync();

            var blobList = new List<BlobInfo>();
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                blobList.Add(new BlobInfo { Name = blobItem.Name, Size = blobItem.Properties.ContentLength , LastModified = blobItem.Properties.LastModified });
            }

            return new OkObjectResult(blobList);
        }
    }

    public class BlobInfo
    {
        public string Name { get; set; }
        public long? Size { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}
