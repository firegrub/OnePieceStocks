using System.ComponentModel.DataAnnotations;

namespace OnePieceStocks.Models
{
    public class AdminAccountToolsModel
    {
        [Required]
        public string Username { get; set; } = "";

        public string? TempPassword { get; set; }

        public bool ForcePasswordChange { get; set; }

        public bool PromoteToAdmin { get; set; }
    }
}