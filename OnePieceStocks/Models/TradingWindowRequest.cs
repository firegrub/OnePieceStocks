using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class TradingWindowRequest
    {
        [Range(0, 365)]
        public int Days { get; set; }

        [Range(0, 168)]
        public int Hours { get; set; }
    }
}