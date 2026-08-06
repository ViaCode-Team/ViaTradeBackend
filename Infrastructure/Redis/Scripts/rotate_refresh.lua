if redis.call('EXISTS', KEYS[1]) == 0 then
	return 0
end

if redis.call('GET', KEYS[2]) ~= ARGV[1] then
	return 0
end

if redis.call('GET', KEYS[3]) ~= ARGV[2] then
	return 0
end

if redis.call('EXISTS', KEYS[4]) ~= 0 or redis.call('EXISTS', KEYS[5]) ~= 0 then
	return 0
end

redis.call('SET', KEYS[1], ARGV[3], 'PX', ARGV[4])
redis.call('SET', KEYS[2], ARGV[5], 'PX', ARGV[4])
redis.call('DEL', KEYS[3])
redis.call('SET', KEYS[4], ARGV[2], 'PX', ARGV[4])
redis.call('SET', KEYS[5], ARGV[2], 'PX', ARGV[6])
redis.call('ZADD', KEYS[6], ARGV[7], ARGV[2])
redis.call('ZADD', KEYS[7], ARGV[8], ARGV[9])

return 1
