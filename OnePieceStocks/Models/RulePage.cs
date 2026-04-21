namespace OnePieceStocks.Models
{
    public class RulePage
    {
        public int Id { get; set; }

        public string Content { get; set; } = "";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}