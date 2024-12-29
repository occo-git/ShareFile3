using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.IO.Pipes;

namespace ShareFile.Services
{
    public class KeyVaultService
    {
        private readonly SecretClient _secretClient;

        public KeyVaultService()
        {
            _secretClient = new SecretClient(
                Configuration.AzureConfig.KeyVaultUri,
                Configuration.AzureConfig.Credential
            );
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }

        public KeyVaultSecret GetSecret(string secretName)
        {
            return _secretClient.GetSecret(secretName);
        }
    }
}
