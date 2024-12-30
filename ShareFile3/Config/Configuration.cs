using Serilog;
using System.Text.Json;

public static class Configuration
{
    public static MainConfiguration MainConfig { get; private set; }
    public static AzureConfiguration AzureConfig { get; private set; }


    private const string CONST_MaxFileSize = "MAX_FILE_SIZE";
    private const string CONST_PixelsPerModule = "PIXELS_PER_MODULE";
    private const string CONST_TenantID = "AZURE_TENANT_ID";
    private const string CONST_ClientID = "AZURE_CLIENT_ID";
    private const string CONST_SecretID = "AZURE_CLIENT_SECRET";
    private const string CONST_KeyVaultUri = "AZURE_KEY_VAULT_URI";

    public static void Init(this IConfiguration configuration)
    {
        if (MainConfig == null)
        {
            var maxFileSize = configuration.GetValue<long>(CONST_MaxFileSize);
            var pixelsPerModule = configuration.GetValue<int>(CONST_PixelsPerModule);
            MainConfig = new MainConfiguration(maxFileSize, pixelsPerModule);
        }
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
        if (MainConfig == null)
            MainConfig = new MainConfiguration(maxFileSize: 104857600, pixelsPerModule: 4);

        if (AzureConfig == null)
            AzureConfig = new AzureConfiguration(tanentId: "test_tanent_id", clientId: "test_client_id", secretId: "test_secret_id", keyVaultUri: "test_key_vault_uri");
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var loggingConfig = configuration.GetSection("Logging").Get<LoggingConfig>();

        if (loggingConfig != null)
            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                builder.AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext() // добавляет контекст к каждой записи
                    .WriteTo.Console(outputTemplate: loggingConfig.ConsoleTemplate)             // Запись в консоль             
                    .WriteTo.File(                                                              // Запись в файл
                        Path.Combine(AppContext.BaseDirectory, loggingConfig.LogFilePath),          // путь к файлу
                        outputTemplate: loggingConfig.FileTemplate,                                 // шаблон сообщения
                        rollingInterval: RollingInterval.Day,                                       // новый файл каждый день
                        fileSizeLimitBytes: loggingConfig.FileSizeLimitBytes,                       // максимальный размер одного файла
                        retainedFileTimeLimit: TimeSpan.FromDays(3),                                // хранить файлы последние 3 дня
                        rollOnFileSizeLimit: true)

                    //.WriteTo.S3("your-s3-bucket-name", "logs/{Date}/log.txt",
                    //    new S3SinkOptions
                    //    {
                    //        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
                    //    })
                    .CreateLogger());
            });

        return services;
    }
}