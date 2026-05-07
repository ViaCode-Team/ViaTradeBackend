using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class UserDto
    {
        [Required]
        public required int Id { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string HashPassword { get; set; }

        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
        [Required]
        public required DateTime RegisterDate { get; set; }
        [MaxLength(512)]

        public string? TgId { get; set; }
    }
}