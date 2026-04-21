using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string ActivityType { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}