using Microsoft.AspNetCore.Mvc;
using BorrowService.Models;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace BorrowService.Controllers
{
    [ApiController]
    [Route("api/borrow")]
    public class BorrowController : ControllerBase
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string filePath = "borrows.json";

        [HttpPost]
        public async Task<IActionResult> BorrowBook(Borrow borrow)
        {
            // 1. Kullanıcı var mı? → UserService
            var userResponse = await _httpClient.GetAsync(
                $"https://localhost:5001/api/users/{borrow.UserId}");
            if (!userResponse.IsSuccessStatusCode)
                return BadRequest("Kullanıcı servisine ulaşılamadı veya kullanıcı bulunamadı");

            // 2. Kitap var mı ve müsait mi? → BookService
            var bookResponse = await _httpClient.GetAsync(
                $"https://localhost:5002/api/books/{borrow.BookId}");
            if (!bookResponse.IsSuccessStatusCode)
                return BadRequest("Kitap servisine ulaşılamadı veya kitap bulunamadı");

            var bookJson = await bookResponse.Content.ReadAsStringAsync();
            var book = JsonSerializer.Deserialize<Book>(bookJson);

            if (book.Status != "Müsait")
                return BadRequest("Kitap şu anda ödünçte");

            // 3. Borrow kaydı oluştur
            List<Borrow> borrows = new();
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                borrows = JsonSerializer.Deserialize<List<Borrow>>(json);
            }

            borrow.BorrowDate = DateTime.Now;
            borrows.Add(borrow);

            var updatedJson = JsonSerializer.Serialize(borrows, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(filePath, updatedJson);

            // 4. Kitap durumunu güncelle → BookService
            await _httpClient.PutAsync(
                $"https://localhost:5002/api/books/{borrow.BookId}/status",
                null);

            return Ok("Kitap başarıyla ödünç alındı");
        }
    }
}
