namespace Domain.Entities.Redis
{
    public class UserRedisEntity : RedisEntity
    {
        public string Login { get; set; } = default!;
        public string? RefreshToken { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
