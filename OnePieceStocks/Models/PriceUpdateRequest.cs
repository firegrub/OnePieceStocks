using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class PriceUpdateRequest
    {
        [Required]
        public int CharacterId { get; set; }

        [Required]
        public string ChangeDirection { get; set; } = "Increase";

        [Required]
        public string ChangeMode { get; set; } = "Percent";

        public string? PresetTier { get; set; }

        public decimal? CustomValue { get; set; }

        public bool IsDeathEvent { get; set; }

        public string? Notes { get; set; }
    }
}