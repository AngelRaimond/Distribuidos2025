import Redis from 'ioredis';
const host = process.env.REDIS_HOST || '127.0.0.1';
const port = Number(process.env.REDIS_PORT || 6379);
export const redis = new Redis({ host, port });
export async function cacheGet(key) {
    const raw = await redis.get(key);
    return raw ? JSON.parse(raw) : null;
}
export async function cacheSet(key, value, ttlSec = 60) {
    await redis.set(key, JSON.stringify(value), 'EX', ttlSec);
}
export async function cacheDel(key) {
    await redis.del(key);
}
// Simple global versioning for broad invalidation without scanning keys
export async function cacheVersion(tag) {
    const v = await redis.get(`v:${tag}`);
    return v ? Number(v) : 0;
}
export async function cacheBump(tag) {
    return await redis.incr(`v:${tag}`);
}
