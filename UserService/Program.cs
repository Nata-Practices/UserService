using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserService.Utils;
using UserService.Services;

namespace UserService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Настройка конфигурации
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Properties/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("Properties/appsettings.Development.json", optional: true, reloadOnChange: true);

        // Отключение автоматической обработки ошибок модели
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        // Настройка логирования
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Конфигурация MongoDB
        builder.Services.Configure<MongoDBSettings>(
            builder.Configuration.GetSection("MongoDB"));

        builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDBSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        // Конфигурация Kafka
        builder.Services.AddSingleton<KafkaProducer>();
        builder.Services.AddSingleton<KafkaConsumer>();

        // Регистрация сервисов
        builder.Services.AddSingleton<IUserService, Services.UserService>();

        // Добавление контроллеров
        builder.Services.AddControllers();

        // Добавление Swagger для API-документации
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Настройка Swagger в режиме разработки
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Регистрируем приём сообщений от Kafka
        var kafkaConsumer = app.Services.GetRequiredService<KafkaConsumer>();
        kafkaConsumer.StartListening(async (key, value) =>
        {
            Console.WriteLine($"[Kafka] Новое сообщение: Key={key}, Value={value}");
            var userService = app.Services.GetRequiredService<IUserService>();

            try
            {
                await userService.IncrementRegisteredObjectsAsync(value, key);
                Console.WriteLine($"[Kafka] Пользователь с ID={value} успешно обновлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Kafka] Ошибка обновления пользователя: {ex.Message}");
            }
        });

        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}
