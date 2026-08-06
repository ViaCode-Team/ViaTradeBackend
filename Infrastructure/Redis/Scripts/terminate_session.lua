local refreshTokenFingerprint = redis.call('GET', KEYS[2])
local removedSessionCount = redis.call('DEL', KEYS[1])

redis.call('DEL', KEYS[2])

if refreshTokenFingerprint then
	redis.call('DEL', ARGV[3] .. refreshTokenFingerprint)
end

redis.call('ZREM', KEYS[3], ARGV[1])
redis.call('ZREM', KEYS[4], ARGV[2])

return removedSessionCount
