using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class TradeHistory
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public int CharacterId { get; set; }
        public Character? Character { get; set; }

        [Required]
        public string TradeType { get; set; } = "";

        public int Quantity { get; set; }

        public decimal PricePerShare { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}