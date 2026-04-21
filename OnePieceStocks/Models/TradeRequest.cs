using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class TradeRequest
    {
        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int CharacterId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string ActionType { get; set; } = "";
    }
}