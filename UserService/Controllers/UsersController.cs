using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

/// <summary>
/// Контроллер для управления пользователями.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Получить список всех пользователей.
    /// </summary>
    /// <returns>Список пользователей</returns>
    /// <response code="200">Успешный ответ с данными пользователей</response>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        Console.WriteLine("Получен список всех пользователей.");
        return Ok(new ApiResponse<IEnumerable<UserModel>>
        {
            Success = true,
            Data = users,
            Message = "Пользователи успешно получены."
        });
    }

    /// <summary>
    /// Получить пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="200">Успешный ответ с данными пользователя</response>
    /// <response code="404">Пользователь с таким идентификатором не найден</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно найден.");
            return Ok(new ApiResponse<UserModel>
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно получен."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new ApiResponse<string>
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }

    /// <summary>
    /// Создать нового пользователя.
    /// </summary>
    /// <param name="user">Модель нового пользователя</param>
    /// <returns>Созданный пользователь</returns>
    /// <response code="201">Пользователь успешно создан</response>
    /// <response code="400">Неверный формат данных</response>
    /// <response code="409">Пользователь с таким ID уже существует</response>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel user)
    {
        try
        {
            await userService.CreateUserAsync(user);
            Console.WriteLine($"Создан пользователь с ID {user.Id}.");
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new ApiResponse<UserModel>
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно создан."
            });
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            Console.WriteLine($"Пользователь с ID {user.Id} уже существует.");
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = $"Пользователь с ID {user.Id} уже существует!"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании пользователя: {ex.Message}");
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = $"Ошибка при создании пользователя: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Обновить данные пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="updatedUser">Модель обновленного пользователя</param>
    /// <returns>Обновленный пользователь</returns>
    /// <response code="200">Пользователь успешно обновлен</response>
    /// <response code="404">Пользователь с таким идентификатором не найден</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModel updatedUser)
    {
        var user = await userService.UpdateUserAsync(id, updatedUser);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно обновлен.");
            return Ok(new ApiResponse<UserModel>
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно обновлен."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new ApiResponse<string>
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }

    /// <summary>
    /// Удалить пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <returns>Удалённый пользователь</returns>
    /// <response code="200">Пользователь успешно удалён</response>
    /// <response code="404">Пользователь с таким идентификатором не найден</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userService.DeleteUserAsync(id);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно удалён.");
            return Ok(new ApiResponse<UserModel>
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно удалён."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new ApiResponse<string>
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }
}
