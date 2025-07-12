using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeartLabVR.AI
{
    /// <summary>
    /// Manages response caching for AI queries to improve performance
    /// Reduces API calls and provides faster responses for repeated queries
    /// </summary>
    public class ResponseCache : MonoBehaviour
    {
        [Header("Cache Configuration")]
        [SerializeField] private int maxCacheSize = 100;
        [SerializeField] private float cacheExpirationHours = 24f;
        [SerializeField] private bool persistCache = true;
        [SerializeField] private bool logCacheActivity = false;

        [System.Serializable]
        public class CachedResponse
        {
            public string query;
            public string anatomicalContext;
            public string response;
            public DateTime timestamp;
            public int accessCount;

            public CachedResponse(string q, string context, string resp)
            {
                query = q;
                anatomicalContext = context;
                response = resp;
                timestamp = DateTime.Now;
                accessCount = 1;
            }

            public bool IsExpired(float expirationHours)
            {
                return (DateTime.Now - timestamp).TotalHours > expirationHours;
            }

            public string GetCacheKey()
            {
                return $"{query}_{anatomicalContext}".ToLowerTurkish().GetHashCode().ToString();
            }
        }

        private Dictionary<string, CachedResponse> responseCache;
        private Queue<string> cacheOrder; // For LRU eviction

        public int CacheSize => responseCache?.Count ?? 0;
        public int CacheHits { get; private set; }
        public int CacheMisses { get; private set; }

        private void Awake()
        {
            responseCache = new Dictionary<string, CachedResponse>();
            cacheOrder = new Queue<string>();
            
            if (persistCache)
            {
                LoadCacheFromDisk();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (persistCache && pauseStatus)
            {
                SaveCacheToDisk();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (persistCache && !hasFocus)
            {
                SaveCacheToDisk();
            }
        }

        private void OnDestroy()
        {
            if (persistCache)
            {
                SaveCacheToDisk();
            }
        }

        /// <summary>
        /// Retrieves a cached response if available and not expired
        /// </summary>
        /// <param name="query">User's query</param>
        /// <param name="anatomicalContext">Anatomical context</param>
        /// <returns>Cached response or null if not found</returns>
        public string GetCachedResponse(string query, string anatomicalContext = "")
        {
            string cacheKey = CreateCacheKey(query, anatomicalContext);
            
            if (responseCache.ContainsKey(cacheKey))
            {
                var cachedItem = responseCache[cacheKey];
                
                // Check if expired
                if (cachedItem.IsExpired(cacheExpirationHours))
                {
                    RemoveFromCache(cacheKey);
                    CacheMisses++;
                    
                    if (logCacheActivity)
                    {
                        Debug.Log($"Cache expired for query: {query}");
                    }
                    
                    return null;
                }

                // Update access info
                cachedItem.accessCount++;
                CacheHits++;

                if (logCacheActivity)
                {
                    Debug.Log($"Cache hit for query: {query} (hits: {cachedItem.accessCount})");
                }

                return cachedItem.response;
            }

            CacheMisses++;
            return null;
        }

        /// <summary>
        /// Caches a new response
        /// </summary>
        /// <param name="query">User's query</param>
        /// <param name="anatomicalContext">Anatomical context</param>
        /// <param name="response">AI response to cache</param>
        public void CacheResponse(string query, string anatomicalContext, string response)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(response))
                return;

            string cacheKey = CreateCacheKey(query, anatomicalContext);
            
            // Remove if already exists (update)
            if (responseCache.ContainsKey(cacheKey))
            {
                RemoveFromCache(cacheKey);
            }

            // Check cache size limit
            if (responseCache.Count >= maxCacheSize)
            {
                EvictOldestEntry();
            }

            // Add new entry
            var cachedResponse = new CachedResponse(query, anatomicalContext, response);
            responseCache[cacheKey] = cachedResponse;
            cacheOrder.Enqueue(cacheKey);

            if (logCacheActivity)
            {
                Debug.Log($"Cached response for query: {query} (cache size: {responseCache.Count})");
            }
        }

        /// <summary>
        /// Clears expired entries from cache
        /// </summary>
        public void CleanExpiredEntries()
        {
            var expiredKeys = new List<string>();
            
            foreach (var kvp in responseCache)
            {
                if (kvp.Value.IsExpired(cacheExpirationHours))
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (string key in expiredKeys)
            {
                RemoveFromCache(key);
            }

            if (logCacheActivity && expiredKeys.Count > 0)
            {
                Debug.Log($"Cleaned {expiredKeys.Count} expired cache entries");
            }
        }

        /// <summary>
        /// Clears all cached responses
        /// </summary>
        public void ClearCache()
        {
            responseCache.Clear();
            cacheOrder.Clear();
            CacheHits = 0;
            CacheMisses = 0;

            if (logCacheActivity)
            {
                Debug.Log("Cache cleared");
            }
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics as string</returns>
        public string GetCacheStats()
        {
            float hitRate = (CacheHits + CacheMisses) > 0 ? 
                (float)CacheHits / (CacheHits + CacheMisses) * 100f : 0f;

            return $"Cache Size: {CacheSize}/{maxCacheSize}, Hit Rate: {hitRate:F1}%, Hits: {CacheHits}, Misses: {CacheMisses}";
        }

        /// <summary>
        /// Gets all cached queries for debugging
        /// </summary>
        /// <returns>List of cached queries</returns>
        public List<string> GetCachedQueries()
        {
            var queries = new List<string>();
            foreach (var response in responseCache.Values)
            {
                queries.Add($"{response.query} [{response.anatomicalContext}] - Access: {response.accessCount}");
            }
            return queries;
        }

        private string CreateCacheKey(string query, string anatomicalContext)
        {
            return $"{query.ToLowerTurkish()}_{anatomicalContext.ToLowerTurkish()}".GetHashCode().ToString();
        }

        private void RemoveFromCache(string cacheKey)
        {
            responseCache.Remove(cacheKey);
            
            // Remove from order queue (rebuild queue without this key)
            var newOrder = new Queue<string>();
            while (cacheOrder.Count > 0)
            {
                string key = cacheOrder.Dequeue();
                if (key != cacheKey && responseCache.ContainsKey(key))
                {
                    newOrder.Enqueue(key);
                }
            }
            cacheOrder = newOrder;
        }

        private void EvictOldestEntry()
        {
            if (cacheOrder.Count > 0)
            {
                string oldestKey = cacheOrder.Dequeue();
                responseCache.Remove(oldestKey);

                if (logCacheActivity)
                {
                    Debug.Log($"Evicted oldest cache entry: {oldestKey}");
                }
            }
        }

        private void SaveCacheToDisk()
        {
            try
            {
                var cacheData = new List<CachedResponse>();
                foreach (var response in responseCache.Values)
                {
                    if (!response.IsExpired(cacheExpirationHours))
                    {
                        cacheData.Add(response);
                    }
                }

                string json = JsonUtility.ToJson(new SerializableList<CachedResponse>(cacheData));
                string filePath = Application.persistentDataPath + "/response_cache.json";
                System.IO.File.WriteAllText(filePath, json);

                if (logCacheActivity)
                {
                    Debug.Log($"Cache saved to disk: {cacheData.Count} entries");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save cache to disk: {ex.Message}");
            }
        }

        private void LoadCacheFromDisk()
        {
            try
            {
                string filePath = Application.persistentDataPath + "/response_cache.json";
                
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    var cacheData = JsonUtility.FromJson<SerializableList<CachedResponse>>(json);

                    foreach (var response in cacheData.items)
                    {
                        if (!response.IsExpired(cacheExpirationHours))
                        {
                            string cacheKey = response.GetCacheKey();
                            responseCache[cacheKey] = response;
                            cacheOrder.Enqueue(cacheKey);
                        }
                    }

                    if (logCacheActivity)
                    {
                        Debug.Log($"Cache loaded from disk: {responseCache.Count} entries");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load cache from disk: {ex.Message}");
            }
        }

        [System.Serializable]
        private class SerializableList<T>
        {
            public List<T> items;
            
            public SerializableList(List<T> list)
            {
                items = list;
            }
        }
    }
}