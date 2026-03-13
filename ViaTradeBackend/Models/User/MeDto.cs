namespace ViaTradeBackend.Models.User
{
    public class MeDto
    { 
        public required int Id { get; set; }
        public required string Login { get; set; }
        public required DateTime LastLoginDate { get; set; }
        public string? TgId { get; set; }
    }
}
