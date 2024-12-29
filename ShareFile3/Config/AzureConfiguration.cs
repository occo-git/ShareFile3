using Azure.Identity;

public class AzureConfiguration
{
    public string TanentID { get; private set; }
    public string ClientID { get; private set; }
    public string SecretID { get; private set; }
    public Uri KeyVaultUri { get; private set; }

    public ClientSecretCredential Credential { get; private set; }

    public AzureConfiguration(string tanentId, string clientId, string secretId, string keyVaultUri) 
    {
        TanentID = tanentId;
        ClientID = clientId;
        SecretID = secretId;
        Credential = new ClientSecretCredential(TanentID, ClientID, SecretID);
        KeyVaultUri = new Uri(keyVaultUri);
    }
}