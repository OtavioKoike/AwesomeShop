using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.CacheStorage
{
    public class RedisService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var objectString = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(objectString))
            {
                Console.WriteLine("cache key not found");
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(objectString);

        }

        public async Task SetAsync<T>(string key, T data)
        {
            var memoryCacheEntryOptions = new DistributedCacheEntryOptions
            {
                // Tempo de expiracao a partir de agora -> Obrigatorio que daqui 3600 vai expirar
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                // Quanto tempo ele passou sem ser acessado -> 1200 sem ser acesado vai expirar
                SlidingExpiration = TimeSpan.FromSeconds(1200)
            };

            var objectString = JsonConvert.SerializeObject(data);
            await _cache.SetStringAsync(key, objectString, memoryCacheEntryOptions);
        }
    }
}
