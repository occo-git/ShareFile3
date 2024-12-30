using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Cosmos;
using MongoDB.Driver;
using ShareFile.Models;
using System.Xml;

namespace ShareFile.Services
{
    public class BlobStorageService
    {
        private const string CONST_ConnectionString_SecretKey = "blob-storage-connection-string";
        private const string CONST_ShareFileContainer = "share-file-bucket";
        private const string CONST_FileRecordsTable = "FileRecords";

        private readonly ILoggerService _log;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorageService(ILoggerService logger, KeyVaultService keyVaultService)
        {
            _log = logger;

            KeyVaultSecret secret = keyVaultService.GetSecret(CONST_ConnectionString_SecretKey);
            _log.Info($"Create Blob Storage client");
            _blobServiceClient = new BlobServiceClient(secret.Value);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(CONST_ShareFileContainer);
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
                    return url;
                }
                catch (Exception ex)
                {
                    _log.Error("Unknown encountered on server when writing an object", ex.Message);
                    throw;
                }
            }
        }
    }
}
