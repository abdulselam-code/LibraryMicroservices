using System;
namespace BorrowService.Models
{
    public class Borrow
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
    }
}


