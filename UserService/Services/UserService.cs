using MongoDB.Driver;
using UserService.Models;
using UserService.Utils;

namespace UserService.Services;

public interface IUserService
{
    Task<List<UserModel>> GetAllUsersAsync(); // Получить всех пользователей
    Task<UserModel> GetUserByIdAsync(string id); // Получить пользователя по ID
    Task CreateUserAsync(UserModel user); // Создать нового пользователя
    Task<UserModel> UpdateUserAsync(string id, UserModel updatedUser); // Обновить данные пользователя
    Task<UserModel> DeleteUserAsync(string id); // Удалить пользователя
    Task IncrementRegisteredObjectsAsync(string userId, string objectId); // Инкремент зарегистрированных объектов
}

public class UserService : IUserService
{
    private readonly IMongoCollection<UserModel> _users;
    private readonly KafkaProducer _kafkaProducer;
    
    public UserService(IMongoClient client, IConfiguration configuration, KafkaProducer kafkaProducer)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _users = database.GetCollection<UserModel>("Users");
        _kafkaProducer = kafkaProducer;
    }

    public async Task<List<UserModel>> GetAllUsersAsync() =>
        await _users.Find(user => true).ToListAsync();

    public async Task<UserModel> GetUserByIdAsync(string id) =>
        await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(UserModel user) =>
        await _users.InsertOneAsync(user);

    public async Task<UserModel> UpdateUserAsync(string id, UserModel updatedUser)
    {
        var result = await _users.ReplaceOneAsync(user => user.Id == id, updatedUser);
        return result.MatchedCount > 0 ? updatedUser : null;
    }

    public async Task<UserModel> DeleteUserAsync(string id)
    {
        var user = await _users.FindOneAndDeleteAsync(user => user.Id == id);
        return user;
    }

    public async Task IncrementRegisteredObjectsAsync(string userId, string objectId)
    {
        var update = Builders<UserModel>.Update.Inc(u => u.RegisteredObjects, 1);
        var result = await _users.UpdateOneAsync(user => user.Id == userId, update);
        
        if (result.ModifiedCount == 0)
        {
            throw new Exception($"Пользователь с id {userId} не найден или не получилось обновиться.");
        }
        else
        {
            var timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 формат
            await _kafkaProducer.SendMessageAsync(objectId, timestamp);
            Console.WriteLine($"[Kafka] Сообщение отправлено: ObjectId={objectId}, Timestamp={timestamp}");
        }
    }
}