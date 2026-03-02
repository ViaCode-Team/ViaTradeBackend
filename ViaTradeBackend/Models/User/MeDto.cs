namespace ViaTradeBackend.Models.User
{
    public class MeDto
    { 
        public int Id { get; set; }
        public required string Login { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string? TgId { get; set; }
    }
}
