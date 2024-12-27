using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        Console.WriteLine("Получен список всех пользователей.");
        return Ok(new
        {
            Success = true,
            Data = users,
            Message = "Пользователи успешно получены."
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно найден.");
            return Ok(new
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно получен."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel user)
    {
        try
        {
            await userService.CreateUserAsync(user);
            Console.WriteLine($"Создан пользователь с ID {user.Id}.");
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно создан."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании пользователя: {ex.Message}");
            return StatusCode(500, new
            {
                Success = false,
                Message = $"Ошибка при создании пользователя: {ex.Message}"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModel updatedUser)
    {
        var user = await userService.UpdateUserAsync(id, updatedUser);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно обновлен.");
            return Ok(new
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно обновлен."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userService.DeleteUserAsync(id);
        if (user != null)
        {
            Console.WriteLine($"Пользователь с ID {id} успешно удален.");
            return Ok(new
            {
                Success = true,
                Data = user,
                Message = "Пользователь успешно удален."
            });
        }

        Console.WriteLine($"Пользователь с ID {id} не найден.");
        return NotFound(new
        {
            Success = false,
            Message = $"Пользователь с ID {id} не найден!"
        });
    }
}
