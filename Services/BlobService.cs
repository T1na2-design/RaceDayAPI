using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Services
{
    /// <summary>
    /// Azure Blob Storage implementation for uploading files to the cloud.
    /// </summary>
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Initializes a new instance of the BlobService.
        /// </summary>
        /// <param name="configuration">Application configuration to read Azure settings.</param>
        /// <exception cref="InvalidOperationException">Thrown if Azure Blob Storage configuration is missing.</exception>
        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:AzureBlobStorage is not configured in appsettings.json");

            var containerName = configuration["AzureBlobStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "AzureBlobStorage:ContainerName is not configured in appsettings.json");

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage and returns the public URL.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <returns>The public URL of the uploaded blob.</returns>
        /// <exception cref="ArgumentNullException">Thrown if file is null or empty.</exception>
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException(nameof(file), "File cannot be null or empty.");
            }

            // Generate a unique blob name to avoid collisions
            string blobName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

            try
            {
                // Upload the file
                BlobClient blobClient = _containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Return the public URL
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to upload file '{file.FileName}' to Azure Blob Storage.", ex);
            }
        }
    }
}
