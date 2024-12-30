using Microsoft.AspNetCore.Http.Features;
using ShareFile.Services;

namespace ShareFile
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configuration
            builder.Configuration.Init();
            #endregion

            #region Logging
            builder.Logging.ClearProviders(); // Удаляем все провайдеры логирования по умолчанию
            builder.Logging.AddConsole(); // Добавляем логирование в консоль
            builder.Logging.AddDebug(); // Добавляем логирование для отладки
            builder.Services.ConfigureLogging(builder.Configuration);
            builder.Services.AddSingleton<ILoggerService, LoggerService>();
            #endregion

            #region CORS
            builder.Services.AddCors(options => // CORS
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
            #endregion

            #region Add services to the container
            builder.Services.AddSingleton<KeyVaultService>();
            builder.Services.AddSingleton<CosmosDbService>();
            builder.Services.AddSingleton<BlobStorageService>();
            builder.Services.AddSingleton<SpeedLinkService>(); // short url service  
            builder.Services.AddScoped<IShareFileService, ShareFileService>(); // Регистрирует сервис FileShare в контейнер зависимостей 
            builder.Services.AddControllersWithViews(); // Добавляет поддержку контроллеров MVC с представлениями            
            builder.Services.AddControllers(); // Добавляет поддержку контроллеров API 
            builder.Services.AddEndpointsApiExplorer(); // Необходим для автоматического обнаружения конечных точек API            
            builder.Services.AddSwaggerGen(); // Добавляет поддержку Swagger для документирования вашего API
            //builder.Services.AddOpenApi(); // Swagger doc gen https://aka.ms/aspnet/openapi
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Configuration.MainConfig.MaxFileSize;
            });
            #endregion

            #region Настройка Kestrel
            //app.Urls.Add("http://localhost:80"); // Настройка HTTP на порту 80
            //app.Urls.Add("https://localhost:443"); // Настройка HTTPS на порту 443, если нужно
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = Configuration.MainConfig.MaxFileSize;
                options.ListenAnyIP(80); // Слушает на порту 80
                //options.ListenAnyIP(443, listenOptions =>
                //{
                //    listenOptions.UseHttps("path/to/certificate.pfx", "your_certificate_password"); // HTTPS
                //});
            });
            #endregion

            #region Authentication, Authorization
            //builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            //builder.Services.AddAuthorization(options =>
            //{
            //    // By default, all incoming requests will be authorized according to the default policy.
            //    options.FallbackPolicy = options.DefaultPolicy;
            //});
            #endregion

            var app = builder.Build();
            // Получаем логгер
            var logger = app.Services.GetRequiredService<ILoggerService>();

            #region Настройка middleware (промежуточных обработчиков)
            if (app.Environment.IsDevelopment())
            {
                logger.Info("IsDevelopment=true");
                // Если приложение находится в режиме разработки, включается Swagger и его интерфейс пользователя для тестирования API
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.MapOpenApi(); // Swagger doc gen
                //app.UseSwaggerUI(c =>
                //{
                //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //    c.RoutePrefix = "swagger";
                //});

                app.UseDeveloperExceptionPage(); // Отображает страницу с ошибками в разработке
            }
            else
            {
                logger.Info("IsDevelopment=false");
                app.UseExceptionHandler("/Home/Error"); // Перенаправляет на страницу ошибки
                //app.UseHsts(); // Включает HSTS (HTTP Strict Transport Security) — веб-протокол безопасности
            }
            //app.UseHttpsRedirection(); // Перенаправляет HTTP-запросы на HTTPS
            app.UseStaticFiles(); // Позволяет обслуживать статические файлы (HTML, CSS, JavaScript, изображения и другие ресурсы, находящиеся в папке wwwroot (по умолчанию))
            #endregion

            //app.UseHttpsRedirection(); // Перенаправляет HTTP-запросы на HTTPS
            app.UseStaticFiles(); // Позволяет обслуживать статические файлы (HTML, CSS, JavaScript, изображения и другие ресурсы, находящиеся в папке wwwroot (по умолчанию))

            #region Настройка маршрутов
            app.UseRouting(); // Настраивает маршрутизацию, позволяя определять, как запросы будут сопоставляться с конечными точками (например, контроллерами и действиями в MVC)            
            app.UseCors("AllowAll"); // Включение CORS (after Routing, before UseAuthentication & UseAuthorization)
            //app.UseAuthentication(); // Проверка аутентификации
            //app.UseAuthorization();  // Проверка авторизации (after UseAuthentication)
            app.MapControllers(); // Настраивает маршруты для контроллеров API
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=HomeView}/{id?}"); // Home и HomeView как значения по умолчанию         
            #endregion

            app.Run();
        }
    }
}
