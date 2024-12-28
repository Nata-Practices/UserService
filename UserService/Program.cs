using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
        
        // Добавляем JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var secretKey = jwtSettings["SecretKey"];

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        
        // Отключение автоматической обработки ошибок модели
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        
        // Настройка логирования
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        // Добавление контроллеров и подключение фильтра для валидации
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
        
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

        // Добавление поддержки Swagger для документации API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "User API",
                Version = "v1",
                Description = "Вторичный API"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Введите JWT токен с префиксом 'Bearer '",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

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
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}
