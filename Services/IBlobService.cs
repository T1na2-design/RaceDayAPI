namespace RaceDay.Services
{
    /// <summary>
    /// Interface for blob storage operations (e.g., Azure Blob Storage).
    /// </summary>
    public interface IBlobService
    {
        /// <summary>
        /// Uploads a file to blob storage and returns the public URL.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <returns>The public URL of the uploaded blob.</returns>
        Task<string> UploadFileAsync(IFormFile file);
    }
}
