using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Cache.Memory
{
    public sealed class MemoryCache : IMemoryCache
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value))
                return (T)value;

            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            if(_cache.ContainsKey(key))
            {
                _cache[key] = value;
            }
            else
            {
                _cache.Add(key, value);
            }
        }
    }
}
