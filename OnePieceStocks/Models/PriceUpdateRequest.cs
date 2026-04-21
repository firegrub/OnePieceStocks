using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class PriceUpdateRequest
    {
        [Required]
        public int CharacterId { get; set; }

        [Required]
        public string ChangeDirection { get; set; } = "Increase";

        public decimal? CustomValue { get; set; }

        public bool IsDeathEvent { get; set; }

        public string? Notes { get; set; }
    }
}