using System.Text.Json.Serialization;
using ViaTrade.Application.Users.Models;

namespace ViaTrade.Infrastructure.Redis.Serialization;

[JsonSerializable(typeof(UserSessionDto))]
internal partial class RedisJsonSerializerContext : JsonSerializerContext;
