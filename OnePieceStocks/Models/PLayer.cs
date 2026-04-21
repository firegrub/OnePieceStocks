using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class Player
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Range(0, double.MaxValue)]
        public decimal Wallet { get; set; }

        [Range(0, 100)]
        public int TotalSlots { get; set; }

        public string? Notes { get; set; }

        public string? ProfileImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Holding> Holdings { get; set; } = new List<Holding>();
        public ICollection<TradeHistory> TradeHistories { get; set; } = new List<TradeHistory>();

        public bool MustChangePassword { get; set; } = true;
    }
}