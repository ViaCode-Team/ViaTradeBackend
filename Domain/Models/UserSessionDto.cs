namespace Domain.Models
{
    public class UserSessionDto
    {
        public string Id { get; set; } = default!;
        public string UserAgent { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
