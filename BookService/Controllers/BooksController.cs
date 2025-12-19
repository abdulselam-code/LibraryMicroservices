using Microsoft.AspNetCore.Mvc;
using BookService.Models;
using System.Text.Json;
using System.Linq;
namespace BookService.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly string filePath = "books.json";

        [HttpPost]
        public IActionResult AddBook([FromBody] Book book)

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


        [HttpGet]
        public IActionResult GetBooks()
        {
            if (!System.IO.File.Exists(filePath))
                return Ok(new List<Book>());

            var json = System.IO.File.ReadAllText(filePath);
            var books = JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();

            return Ok(books);
        }


        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id)
        {
            var json = System.IO.File.ReadAllText("books.json");
            var books = JsonSerializer.Deserialize<List<Book>>(json);

            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();

            book.Status = "Ödünçte";

            System.IO.File.WriteAllText("books.json",
                JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true }));

            return Ok();
        }

        [HttpPut("{id}/available")]
        public IActionResult MakeAvailable(int id)
        {
            if (!System.IO.File.Exists("books.json"))
                return NotFound("books.json bulunamadı");

            var json = System.IO.File.ReadAllText("books.json");
            var books = JsonSerializer.Deserialize<List<Book>>(json);

            var book = books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound("Kitap bulunamadı");

            book.Status = "Müsait";

            var updatedJson = JsonSerializer.Serialize(books, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText("books.json", updatedJson);

            return Ok("Kitap tekrar müsait duruma getirildi");
        }



    }
}
