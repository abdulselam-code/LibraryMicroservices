using Microsoft.AspNetCore.Mvc;
using BookService.Models;
using System.Text.Json;

namespace BookService.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly string filePath = "books.json";

        [HttpPost]
        public IActionResult AddBook(Book book)
        {
            List<Book> books = new List<Book>();

            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                books = JsonSerializer.Deserialize<List<Book>>(json);
            }

            book.Status = "Müsait";
            books.Add(book);

            var updatedJson = JsonSerializer.Serialize(books, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, updatedJson);

            return Ok("Kitap başarıyla eklendi");
        }
    }
}
