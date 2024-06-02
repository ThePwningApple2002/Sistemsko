using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Projekat_DrugiDeo
{
    public class Cache
    {
        private readonly int capacity;
        private readonly ConcurrentDictionary<string, string> cache;
        private readonly LinkedList<string> lruList;
        private readonly object lockObject = new object();

        public Cache(int capacity = 10)
        {
            this.capacity = capacity;
            cache = new ConcurrentDictionary<string, string>();
            lruList = new LinkedList<string>();
        }

        public void Set(string key, string value)
        {
            lock (lockObject)
            {
                if (cache.ContainsKey(key))
                {
                    lruList.Remove(key);
                }
                else if (cache.Count >= capacity)
                {
                    var oldestKey = lruList.Last.Value;
                    lruList.RemoveLast();
                    cache.TryRemove(oldestKey, out _);
                }

                cache[key] = value;
                lruList.AddFirst(key);
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            lock (lockObject)
            {
                if (cache.TryGetValue(key, out value))
                {
                    lruList.Remove(key);
                    lruList.AddFirst(key);
                    return true;
                }

                return false;
            }
        }
    }
}
