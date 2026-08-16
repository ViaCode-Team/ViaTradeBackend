namespace ViaTrade.Application.Common.Models;

public abstract class CacheEntity
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
}
