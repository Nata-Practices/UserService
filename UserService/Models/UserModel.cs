using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserService.Models;

/// <summary>
/// Модель пользователя.
/// </summary>
public class UserModel
{
    /// <summary>
    /// Уникальный идентификатор пользователя, используется как ключ в базе данных.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [Required(ErrorMessage = "Поле 'Id' обязательно для заполнения.")]
    public string Id { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    [Required(ErrorMessage = "Поле 'Name' обязательно для заполнения.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Имя должно быть от 3 до 100 символов.")]
    public string Name { get; set; }

    /// <summary>
    /// Количество зарегистрированных объектов, связанных с пользователем.
    /// </summary>
    [Required(ErrorMessage = "Поле 'RegisteredObjects' обязательно для заполнения.")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество зарегистрированных объектов не может быть отрицательным.")]
    public int RegisteredObjects { get; set; } = 0;
}