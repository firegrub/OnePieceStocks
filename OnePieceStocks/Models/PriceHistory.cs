using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public Character? Character { get; set; }

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }

        [Required]
        public string ChangeDirection { get; set; } = "";

        [Required]
        public string ChangeMode { get; set; } = "";

        public decimal ChangeValue { get; set; }

        public string? PresetTier { get; set; }

        public bool IsDeathEvent { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}