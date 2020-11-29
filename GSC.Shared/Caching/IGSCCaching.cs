using System;
using System.Collections.Generic;

namespace GSC.Shared.Caching
{
    public interface IGSCCaching
    {
        bool TryGetValue<T>(string key, out T value);
        void Remove(string key);
        void Add<T>(string key, T value, DateTime expiredAfter);
        List<CachingModel> GetAllCachedItems();
        void Reset();
    }
}