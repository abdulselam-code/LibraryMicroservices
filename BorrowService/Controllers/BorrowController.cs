using BorrowService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/borrow")]
public class BorrowController : ControllerBase
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string filePath = "borrows.json";

    // 📌 KİTAP ÖDÜNÇ ALMA
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
            borrows = JsonSerializer.Deserialize<List<Borrow>>(json) ?? new();
        }

        borrow.BorrowDate = DateTime.Now;
        borrows.Add(borrow);

        System.IO.File.WriteAllText(filePath,
            JsonSerializer.Serialize(borrows, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        await _httpClient.PutAsync(
            $"http://localhost:5191/api/books/{borrow.BookId}/status",
            null);

        return Ok("Kitap başarıyla ödünç alındı");
    }

    // 📌 KİTAP İADE
    [HttpPost("/api/return")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnRequest request)
    {
        if (!System.IO.File.Exists(filePath))
            return BadRequest("Ödünç kaydı bulunamadı");

        var json = System.IO.File.ReadAllText(filePath);
        var borrows = JsonSerializer.Deserialize<List<Borrow>>(json) ?? new();

        var borrowRecord = borrows.FirstOrDefault(b =>
            b.UserId == request.UserId &&
            b.BookId == request.BookId);

        if (borrowRecord == null)
            return NotFound("Bu kitap ödünç alınmamış");

        // 📌 Ceza hesaplama
        var borrowedDays = (DateTime.Now - borrowRecord.BorrowDate).Days;
        decimal penalty = borrowedDays > 7 ? (borrowedDays - 7) * 5 : 0;

        // 📌 Borrow kaydı sil
        borrows.Remove(borrowRecord);
        System.IO.File.WriteAllText(filePath,
            JsonSerializer.Serialize(borrows, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        // 📌 BookService → müsait yap
        await _httpClient.PutAsync(
            $"http://localhost:5191/api/books/{request.BookId}/available",
            null);

        return Ok(new
        {
            message = "Kitap başarıyla iade edildi",
            penalty = penalty
        });
    }
}
