namespace OnePieceStocks.Models
{
    public class Holding
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public int CharacterId { get; set; }
        public Character? Character { get; set; }

        public int Quantity { get; set; }

        public decimal AverageBuyPrice { get; set; }
    }
}