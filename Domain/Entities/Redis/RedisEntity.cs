namespace Domain.Entities.Redis
{
    public abstract class RedisEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
