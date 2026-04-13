namespace Domain.Models.Dto
{
    public record UserDto(string Login, string HashPassword, DateTime LastLoginDate, string? TgId);
}
