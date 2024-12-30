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
        private readonly BlobServiceClient _blobServiceClient;
        private CosmosDbService _cosmosDbService;
        private readonly SpeedLinkService _speedLinkService;
        //private ITableFactory _tableFactory;

        private const string CONST_ConnectionString_SecretKey = "blob-storage-connection-string";
        private const string CONST_ShareFileContainer = "share-file-bucket";
        private const string CONST_FileRecordsTable = "FileRecords";

        public ShareFileService(ILoggerService logger, KeyVaultService keyVaultService, CosmosDbService cosmosDbService, SpeedLinkService speedLinkService)
        {
            _log = logger;
            _cosmosDbService = cosmosDbService;
            _speedLinkService = speedLinkService;

            KeyVaultSecret secret = keyVaultService.GetSecret(CONST_ConnectionString_SecretKey);
            _log.Info($"Create Blob Storage client");
            _blobServiceClient = new BlobServiceClient(secret.Value);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, ShareFileModel model)
        {
            using (_log.Scoped(nameof(UploadFileAsync)))
            {
                try
                {
                    _log.Info($"Upload file to Blob Storage: upload [{model}]");
                    _log.Info($"Upload file to Blob Storage: container");
                    var containerClient = _blobServiceClient.GetBlobContainerClient(CONST_ShareFileContainer);
                    await containerClient.CreateIfNotExistsAsync();

                    _log.Info($"Upload file to Blob Storage: blob client");
                    fileStream.Position = 0;
                    var blobClient = containerClient.GetBlobClient(model.UniqueFileName);
                    await blobClient.UploadAsync(fileStream, true);

                    _log.Info($"Upload file to Blob Storage: url");
                    string url = blobClient.Uri.ToString();
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
