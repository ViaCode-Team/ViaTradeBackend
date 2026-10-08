using System.Text.Json.Serialization;
using ViaTrade.Application.Auth.Common;

namespace ViaTrade.Infrastructure.Redis.Serialization;

[JsonSerializable(typeof(SessionData))]
internal partial class RedisJsonSerializerContext : JsonSerializerContext;
