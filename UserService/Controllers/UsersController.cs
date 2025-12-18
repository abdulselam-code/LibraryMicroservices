using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using System.Text.Json;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly string filePath = "users.json";

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            List<User> users = new List<User>();

            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                users = JsonSerializer.Deserialize<List<User>>(json);
            }

            users.Add(user);

            var updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, updatedJson);

            return Ok("Kullanıcı başarıyla eklendi");
        }
    }
}
