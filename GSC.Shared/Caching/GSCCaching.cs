using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace GSC.Shared.Caching
{
    public class GSCCaching : IGSCCaching
    {
        private readonly IMemoryCache _cache;

        public GSCCaching(IMemoryCache cache)
        {
            _cache = cache;
        }
        public bool TryGetValue<T>(string key, out T value)
        {
            return _cache.TryGetValue(key.ToLower(), out value);
        }

        public void Add<T>(string key, T value, DateTime expiredAfter)
        {
            _cache.Set(key.ToLower(), value, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiredAfter,
                SlidingExpiration = TimeSpan.FromDays(1)
            });
        }
        public void Remove(string key)
        {
            _cache.Remove(key.ToLower());
        }
        public void Reset()
        {
            var items = GetAllCachedItems();
            foreach (var item in items)
                _cache.Remove(item.Key);
        }

        public List<CachingModel> GetAllCachedItems()
        {
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(_cache) as ICollection;
            var items = new List<CachingModel>();
            if (collection != null)
                foreach (var item in collection)
                {
                    var methodInfo = item.GetType().GetProperty("Key");
                    var key = methodInfo.GetValue(item);
                    TryGetValue(key.ToString(), out object value);

                    items.Add(new CachingModel
                    {
                        Key = key.ToString(),
                        value = value
                    });
                }

            return items;
        }
    }
}