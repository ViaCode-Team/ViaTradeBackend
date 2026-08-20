using System.Text.Json.Serialization;
using ViaTrade.Application.Users.Models;
using ViaTrade.Infrastructure.Redis.Entities;

namespace ViaTrade.Infrastructure.Redis.Serialization;

[JsonSerializable(typeof(UserSessionDto))]
[JsonSerializable(typeof(TelegramTokenEntity))]
internal partial class RedisJsonSerializerContext : JsonSerializerContext;
