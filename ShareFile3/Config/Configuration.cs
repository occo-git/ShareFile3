using System.Text.Json;

public static class Configuration
{
    public static AzureConfiguration AzureConfig { get; private set; }

    private const string CONST_TenantID = "AZURE_TENANT_ID";
    private const string CONST_ClientID = "AZURE_CLIENT_ID";
    private const string CONST_SecretID = "AZURE_CLIENT_SECRET";
    private const string CONST_KeyVaultUri = "AZURE_KEY_VAULT_URI";

    public static void Init(this IConfiguration configuration)
    {
        if (AzureConfig == null)
        {
            var tanentId = configuration[CONST_TenantID] ?? string.Empty;
            var clientId = configuration[CONST_ClientID] ?? string.Empty;
            var secretId = configuration[CONST_SecretID] ?? string.Empty;
            var keyVaultUri = configuration[CONST_KeyVaultUri] ?? string.Empty;
            AzureConfig = new AzureConfiguration(tanentId, clientId, secretId, keyVaultUri);
        }
    }

    public static void TestInit()
    {
        if (AzureConfig == null)
            AzureConfig = new AzureConfiguration(tanentId: "test_tanent_id", clientId: "test_client_id", secretId: "test_secret_id", keyVaultUri: "test_key_vault_uri");
    }
}