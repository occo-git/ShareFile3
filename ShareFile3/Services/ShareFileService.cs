using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using ShareFile.Models;

namespace ShareFile.Services
{
    public interface IShareFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, ShareFileModel model);
        Task SaveFileMetadataAsync(ShareFileModel model);
    }

    public class ShareFileService : IShareFileService
    {
        private readonly ILoggerService _log;
        private readonly CosmosDbService _cosmosDbService;
        private readonly BlobStorageService _blobStorageService;
        private readonly SpeedLinkService _speedLinkService;

        public ShareFileService(
            ILoggerService logger, 
            KeyVaultService keyVaultService,
            CosmosDbService cosmosDbService,
            BlobStorageService blobStorageService,
            SpeedLinkService speedLinkService)
        {
            _log = logger;
            _cosmosDbService = cosmosDbService;
            _blobStorageService = blobStorageService;
            _speedLinkService = speedLinkService;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, ShareFileModel model)
        {
            using (_log.Scoped(nameof(UploadFileAsync)))
            {
                try
                {
                    string url = await _blobStorageService.UploadFileAsync(fileStream, model);
                    var shortUrl = await _speedLinkService.CreateShortUrlAsync(url);
                    url = string.IsNullOrEmpty(shortUrl) ? url : shortUrl;
                    return url;
                }
                catch (Exception ex)
                {
                    _log.Error("Unknown encountered on server when writing an object", ex.Message);
                    throw;
                }
            }
        }

        public async Task SaveFileMetadataAsync(ShareFileModel model)
        {
            using (_log.Scoped(nameof(SaveFileMetadataAsync)))
            {
                await _cosmosDbService.SaveFileMetadataAsync(model);
            }
        }
    }
}
