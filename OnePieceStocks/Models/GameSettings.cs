namespace OnePieceStocks.Models
{
    public class GameSettings
    {
        public int Id { get; set; }

        public bool UsePercentByDefault { get; set; } = true;

        public decimal MinorFlat { get; set; } = 500;
        public decimal MajorFlat { get; set; } = 1500;
        public decimal MassiveFlat { get; set; } = 5000;

        public decimal MinorPercent { get; set; } = 5;
        public decimal MajorPercent { get; set; } = 10;
        public decimal MassivePercent { get; set; } = 15;

        public decimal Slot7Price { get; set; } = 50000;
        public decimal Slot8Price { get; set; } = 150000;
        public decimal Slot9Price { get; set; } = 450000;
        public decimal Slot10Price { get; set; } = 1000000;

        public decimal SlotSellPrice { get; set; } = 25000;
        public decimal SlotRebuyPrice { get; set; } = 30000;

        public int MaxSlots { get; set; } = 10;

        public bool IsTradingOpen { get; set; } = false;
        public DateTime? TradingClosesAtUtc { get; set; }
    }
}