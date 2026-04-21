using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class CreatePlayerAccountModel
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Username { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string TempPassword { get; set; } = "";

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Wallet { get; set; }

        [Required]
        [Range(0, 100)]
        public int TotalSlots { get; set; }

        public string? Notes { get; set; }

        public string? ProfileImagePath { get; set; }
    }
}