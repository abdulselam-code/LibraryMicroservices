using BorrowService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/borrow")]
public class BorrowController : ControllerBase
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string filePath = "borrows.json";

    [HttpPost]
    public async Task<IActionResult> BorrowBook([FromBody] Borrow borrow)
    {
        if (borrow == null)
            return BadRequest("Borrow verisi boş");

        // UserService kontrolü
        var userResponse = await _httpClient.GetAsync("http://localhost:5174/api/users");


        if (!userResponse.IsSuccessStatusCode)
            return BadRequest("Kullanıcı servisine ulaşılamadı");

        // BookService kontrolü
        var bookResponse = await _httpClient.GetAsync("http://localhost:5191/api/books");
        if (!bookResponse.IsSuccessStatusCode)
            return BadRequest("Kitap servisine ulaşılamadı");

        List<Borrow> borrows = new();

        if (System.IO.File.Exists(filePath))
        {
            var json = System.IO.File.ReadAllText(filePath);
            borrows = JsonSerializer.Deserialize<List<Borrow>>(json) ?? new List<Borrow>();
        }

        borrow.BorrowDate = DateTime.Now;
        borrows.Add(borrow);

        var updatedJson = JsonSerializer.Serialize(borrows, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        System.IO.File.WriteAllText(filePath, updatedJson);

        await _httpClient.PutAsync(
            $"http://localhost:5191/api/books/{borrow.BookId}/status",
            null);

        return Ok("Kitap başarıyla ödünç alındı");

        [HttpPost("/api/return")]
        public async Task<IActionResult> ReturnBook(ReturnRequest request)
        {
            List<Borrow> borrows = new();

            if (!System.IO.File.Exists("borrows.json"))
                return BadRequest("Ödünç kaydı bulunamadı");

            var json = System.IO.File.ReadAllText("borrows.json");
            borrows = JsonSerializer.Deserialize<List<Borrow>>(json);

            var borrowRecord = borrows.FirstOrDefault(b =>
                b.UserId == request.UserId &&
                b.BookId == request.BookId);

            if (borrowRecord == null)
                return NotFound("Bu kitap ödünç alınmamış");

            // 1️⃣ Gecikme & ceza hesaplama
            var returnDate = DateTime.Now;
            var allowedDays = 7;
            var borrowedDays = (returnDate - borrowRecord.BorrowDate).Days;

            decimal penalty = 0;
            if (borrowedDays > allowedDays)
            {
                penalty = (borrowedDays - allowedDays) * 5;
            }

            // 2️⃣ Borrow kaydı sil
            borrows.Remove(borrowRecord);

            System.IO.File.WriteAllText("borrows.json",
                JsonSerializer.Serialize(borrows, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

            // 3️⃣ BookService → kitap müsait yap
            await _httpClient.PutAsync(
                $"http://localhost:5191/api/books/{request.BookId}/available",
                null);

            return Ok(new
            {
                Message = "Kitap başarıyla iade edildi",
                Penalty = penalty
            });
        }


    }
}
