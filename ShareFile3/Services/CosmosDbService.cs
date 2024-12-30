using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using MongoDB.Driver;
using ShareFile.Models;
using System.Xml;

namespace ShareFile.Services
{
    public class CosmosDbService
    {
        private const string CONST_ConnectionString_SecretKey = "cosmos-db-connection-string";
        private const string CONST_DataBase_Name = "sharefile-db";
        private const string CONST_Collection_Name = "FileRecords";

        private readonly ILoggerService _log;
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;   

        public CosmosDbService(ILoggerService logger, KeyVaultService keyVaultService)
        {
            _log = logger;

            KeyVaultSecret secret = keyVaultService.GetSecret(CONST_ConnectionString_SecretKey);
            _log.Info($"Create Mongo client");
            _mongoClient = new MongoClient(secret.Value);
            _log.Info($"Get database");
            _database = _mongoClient.GetDatabase(CONST_DataBase_Name);
        }

        //public async Task<ShareFileModel> GetShareFileAsync(string url)
        //{
        //    var collection = _database.GetCollection<ShareFileModel>(CONST_Collection_Name);
        //    var filter = Builders<ShareFileModel>.Filter.Eq(u => u.Url, url);

        //    return await collection.Find(filter).FirstOrDefaultAsync();
        //}

        public async Task SaveFileMetadataAsync(ShareFileModel model)
        {
            _log.Info($"Save metadata in CosmosDB: load table {CONST_Collection_Name}");
            var collection = _database.GetCollection<ShareFileModel>(CONST_Collection_Name);
            _log.Info($"Save metadata in CosmosDB: insert [{model}]");
            await collection.InsertOneAsync(model);
            _log.Info($"Save metadata in CosmosDB: insert OK [{model}]");
        }
    }
}
