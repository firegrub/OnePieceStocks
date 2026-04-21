using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class Character
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? ImagePath { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bounty { get; set; }

        public decimal LifetimeIncreaseAmount { get; set; }
        public decimal LifetimeDecreaseAmount { get; set; }
        public decimal WeeklyIncreaseAmount { get; set; }
        public decimal WeeklyDecreaseAmount { get; set; }

        public int LifetimeIncreaseCount { get; set; }
        public int LifetimeDecreaseCount { get; set; }
        public int WeeklyIncreaseCount { get; set; }
        public int WeeklyDecreaseCount { get; set; }

        public int MaxStockUnits { get; set; } = 2000;

        public string? Notes { get; set; }

        public bool IsDead { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Holding> Holdings { get; set; } = new List<Holding>();
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
        public ICollection<TradeHistory> TradeHistories { get; set; } = new List<TradeHistory>();
    }
}