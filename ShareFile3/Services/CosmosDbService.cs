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
        private const string CONST_DataBase_Name = "swiftlink-db";
        private const string CONST_Collection_Name = "LinkRecords";

        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;   

        public CosmosDbService(KeyVaultService keyVaultService)
        {
            KeyVaultSecret secret = keyVaultService.GetSecret(CONST_ConnectionString_SecretKey);
            _mongoClient = new MongoClient(secret.Value);
            _database = _mongoClient.GetDatabase(CONST_DataBase_Name);
        }

        public async Task<SwiftLinkModel> GetSwiftLinkAsync(string shortUrl)
        {
            var collection = _database.GetCollection<SwiftLinkModel>(CONST_Collection_Name);
            var filter = Builders<SwiftLinkModel>.Filter.Eq(u => u.ShortUrl, shortUrl);

            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddSwiftLinkAsync(SwiftLinkModel swiftLink)
        {
            var collection = _database.GetCollection<SwiftLinkModel>(CONST_Collection_Name);
            await collection.InsertOneAsync(swiftLink);
        }
    }
}
